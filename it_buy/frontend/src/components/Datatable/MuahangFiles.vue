<template>
    <div id="TableMuahangFiles">
        <DataTable showGridlines :value="files" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
            <template #header>
                <div class="d-inline-flex" style="width:200px">
                    <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                        @click="addRow"></Button>
                </div>
                <div class="d-inline-flex float-right">
                </div>
            </template>

            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false" :class="col.data">

                <template #body="slotProps">

                    <template v-if="col.data == 'note'">
                        <a target="_blank" :href="slotProps.data['link']"
                            :class="{ 'text-blue': slotProps.data['link'] }">{{ slotProps.data[col.data]
                            }}</a>
                    </template>
                    <template v-else-if="col.data == 'file'">
                        <div class="mt-2" v-for="(item, index) in slotProps.data['list_file']">
                            <a target="_blank" :href="item.url" class="text-blue" :download="item.name">{{ item.name
                                }}</a>
                        </div>
                    </template>
                    <template v-else-if="col.data == 'created_at'">
                        {{ formatDate(slotProps.data[col.data]) }}
                    </template>
                    <template v-else-if="col.data == 'action' && slotProps.data['is_user_upload'] == true">
                        <a class="p-link text-danger font-16" @click="confirmDeleteProduct(slotProps.data)"><i
                                class="pi pi-trash"></i></a>
                    </template>
                    <template v-else>
                        {{ slotProps.data[col.data] }}
                    </template>
                </template>
            </Column>
        </DataTable>
    </div>
</template>

<script setup>

import { onMounted, ref, watch, computed } from 'vue';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import { storeToRefs } from 'pinia'

import { useConfirm } from "primevue/useconfirm";
import { rand } from '../../utilities/rand'
import { useMuahang } from '../../stores/muahang';
import muahangApi from '../../api/muahangApi';
import { useRoute } from 'vue-router';
import { formatDate } from '../../utilities/util';

const store_muahang = useMuahang();
const confirm = useConfirm();

const { files } = storeToRefs(store_muahang);
const route = useRoute();
const loading = ref(false);
const selected = ref();
const columns = ref([
    {
        label: "Mô tả",
        "data": "note",
        "className": "text-center",
    },
    {
        label: "Tập tin",
        "data": "file",
        "className": "text-center",
    },
    {
        label: "Ngày tạo",
        "data": "created_at",
        "className": "text-center"
    },
    {
        label: "Hành động",
        "data": "action",
        "className": "text-center"
    }
])
const addRow = () => {

}
const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
onMounted(async () => {
    var res = await muahangApi.getFiles(route.params.id);
    files.value = res;
})
</script>
