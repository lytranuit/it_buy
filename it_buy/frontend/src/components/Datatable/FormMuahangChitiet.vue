<template>
    <div id="TablemuahangChitiet">
        <DataTable showGridlines :value="datatable" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
            <!-- <template #header>
                <div class="d-inline-flex" style="width:200px" v-if="model.status_id == 1">
                    <Button label="Thêm" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                        @click="addRow"></Button>
                    <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm"
                        :disabled="!selected || !selected.length" @click="confirmDeleteSelected"></Button>

                </div>
                <div class="d-inline-flex float-right">
                </div>
            </template> -->
            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false" :class="col.data">
                <template #body="slotProps">

                    <template v-if="col.data == 'soluong' && model.status_id == 1">
                        <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :maxFractionDigits="2" />
                    </template>
                    <template v-else-if="col.data == 'note' && model.status_id == 1">
                        <textarea v-model="slotProps.data[col.data]" class="form-control" />
                    </template>
                    <template v-else-if="col.data == 'dvt' && model.status_id == 1">
                        {{ slotProps.data["dvt"] }} <i class="fas fa-sync-alt" style="cursor: pointer;"
                            @click="openOp($event, slotProps.data)"></i>
                    </template>
                    <template v-else-if="col.data == 'hh_id'">
                        {{ slotProps.data["mahh"] }}
                    </template>
                    <template v-else>
                        {{ slotProps.data[col.data] }}
                    </template>
                </template>
            </Column>
        </DataTable>
        <OverlayPanel ref="op">
            <div>
                Qui đổi từ 1
                <input class="form-control form-control-sm d-inline-block" style="width: 60px"
                    v-model="editingRow.dvt" />
                = <input class="form-control form-control-sm d-inline-block" style="width: 60px"
                    v-model="editingRow.quidoi" />
                <b>{{ editingRow.dvt_dutru }}</b>

            </div>
        </OverlayPanel>
    </div>
</template>
<script setup>

import { onMounted, ref, watch, computed } from 'vue';
import Materials from "../../components/TreeSelect/MaterialTreeSelect.vue"
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import OverlayPanel from 'primevue/overlaypanel';
import { storeToRefs } from 'pinia'

import { useConfirm } from "primevue/useconfirm";
import { rand } from '../../utilities/rand'
import { formatPrice } from '../../utilities/util'
import { useMuahang } from '../../stores/muahang';
import { useGeneral } from '../../stores/general';

const store_muahang = useMuahang();
const store_general = useGeneral();
const { datatable, model } = storeToRefs(store_muahang);
const editingRow = ref();

const loading = ref(false);
const selected = ref();
const columns = ref([
    {
        label: "STT(*)",
        data: "stt",
        className: "text-center",
    },
    {
        label: "Mã(*)",
        "data": "hh_id",
        "className": "text-center",
    },
    {
        label: "Tên(*)",
        "data": "tenhh",
        "className": "text-center"
    },
    {
        label: "ĐVT(*)",
        "data": "dvt",
        "className": "text-center",
    },
    {
        label: "Số lượng(*)",
        "data": "soluong",
        "className": "text-center"
    },
    {
        label: "Mô tả",
        "data": "note",
        "className": "text-center"
    }
])

const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const op = ref();
const openOp = (event, row) => {
    editingRow.value = row;
    op.value.toggle(event);
}
onMounted(() => {
})
</script>
<style>
.hh_id {
    max-width: 300px;

}
</style>