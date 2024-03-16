<template>
    <div id="TablemuahangNhanhang">
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
            <Column v-for="(col, index) in columns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false" :class="col.data">

                <template #body="slotProps">

                    <template v-if="col.data == 'hh_id'">
                        {{ slotProps.data["mahh"] }}
                    </template>
                    <template v-else-if="editable == true && col.data == 'date_nhanhang'">
                        <Calendar v-model="slotProps.data[col.data]" dateFormat="yy-mm-dd" class="date-custom"
                            :manualInput="false" showIcon :minDate="minDate" :readonly="model.date_finish" />
                    </template>

                    <template v-else-if="editable == true && col.data == 'soluong_nhanhang'">
                        <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm"
                            :readonly="model.date_finish" :maxFractionDigits="2" />
                    </template>

                    <template v-else-if="editable == true && col.data == 'note_nhanhang'">
                        <textarea v-model="slotProps.data[col.data]" class="form-control form-control-sm"
                            :readonly="model.date_finish"></textarea>
                    </template>

                    <template v-else-if="editable == true && col.data == 'status_nhanhang'">
                        <select class="form-control form-control-sm" v-model="slotProps.data[col.data]"
                            :readonly="model.date_finish">
                            <option value="0">Chưa nhận hàng</option>
                            <option value="1">Đã nhận hàng</option>
                            <option value="2">Khiếu nại</option>
                        </select>
                    </template>


                    <template v-else-if="col.data == 'status_nhanhang' && slotProps.data[col.data] == 1">
                        <Chip label="Đã nhận hàng" icon="pi pi-check" class="bg-success text-white" />
                    </template>

                    <template v-else-if="col.data == 'status_nhanhang' && slotProps.data[col.data] == 2">
                        <Chip label="Khiếu nại" icon="pi pi-times" class="bg-danger text-white" />
                    </template>

                    <template v-else-if="col.data == 'status_nhanhang'">
                        <Chip label="Chưa nhận hàng" icon="fas fa-spinner fa-spin" />
                    </template>

                    <template v-else-if="col.data == 'date_nhanhang' && slotProps.data[col.data]">
                        {{ formatDate(slotProps.data[col.data]) }}
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
import Materials from "../../components/TreeSelect/MaterialTreeSelect.vue";
import Chip from 'primevue/chip';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import InputNumber from 'primevue/inputnumber';
import { storeToRefs } from 'pinia'

import { useConfirm } from "primevue/useconfirm";
import { rand } from '../../utilities/rand'
import { formatDate, formatPrice } from '../../utilities/util'
import { useMuahang } from '../../stores/muahang';

const store_muahang = useMuahang();
const { datatable, model } = storeToRefs(store_muahang);
const minDate = ref(new Date());
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
    }, {
        label: "Số lượng nhận hàng",
        "data": "soluong_nhanhang",
        "className": "text-center"
    },
    {
        label: "Ngày nhận hàng",
        "data": "date_nhanhang",
        "className": "text-center"
    },
    {
        label: "Ghi chú",
        "data": "note_nhanhang",
        "className": "text-center"
    },
    {
        label: "Trạng thái",
        "data": "status_nhanhang",
        "className": "text-center"
    }
])

const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const props = defineProps({
    editable: {
        type: Boolean,
        default: false,
    },
})
onMounted(() => {
    // console.log(datatable.value);
})
</script>

<style>
.hh_id {
    max-width: 300px;

}
</style>