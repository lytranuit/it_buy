<template>
    <div id="TablemuahangNhanhang">
        <DataTable showGridlines :value="modelncc" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
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

                    <template v-else-if="col.data == 'date_nhanhang'">
                        <Calendar v-model="slotProps.data[col.data]" dateFormat="yy-mm-dd" class="date-custom"
                            :manualInput="false" showIcon :minDate="minDate" :readonly="model.date_finish"/>
                    </template>

                    <template v-else-if="col.data == 'soluong_nhanhang'">
                        <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :readonly="model.date_finish" :maxFractionDigits="2"/>
                    </template>

                    <template v-else-if="col.data == 'note_nhanhang'">
                        <textarea v-model="slotProps.data[col.data]" class="form-control form-control-sm" :readonly="model.date_finish"></textarea>
                    </template>

                    <template v-else-if="col.data == 'status_nhanhang'">
                        <select class="form-control form-control-sm" v-model="slotProps.data[col.data]" :readonly="model.date_finish">
                            <option value="0">Chưa nhận hàng</option>
                            <option value="1">Đã nhận hàng</option>
                            <option value="2">Khiếu nại</option>
                        </select>
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
import InputNumber from 'primevue/inputnumber';
import { storeToRefs } from 'pinia'

import { useDutru } from '../../stores/dutru';

import Calendar from "primevue/calendar";
const store_dutru = useDutru();
const { list_nhanhang, model } = storeToRefs(store_dutru);

const minDate = ref(new Date());
const loading = ref(false);
const selected = ref();
const columns = ref([
    {
        label: "Mã",
        "data": "hh_id",
        "className": "text-center",
    },
    {
        label: "Tên",
        "data": "tenhh",
        "className": "text-center"
    },
    {
        label: "ĐVT",
        "data": "dvt",
        "className": "text-center",
    },
    {
        label: "Số lượng",
        "data": "soluong",
        "className": "text-center"
    },
    {
        label: "Số lượng nhận hàng (*)",
        "data": "soluong_nhanhang",
        "className": "text-center"
    },
    {
        label: "Ngày nhận hàng (*)",
        "data": "date_nhanhang",
        "className": "text-center"
    },
    {
        label: "Ghi chú (*)",
        "data": "note_nhanhang",
        "className": "text-center"
    },
    {
        label: "Trạng thái (*)",
        "data": "status_nhanhang",
        "className": "text-center"
    }
])

const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const props = defineProps({
    index: Number,
})
const modelncc = computed(() => {
    return list_nhanhang.value[props.index].items;
});
onMounted(() => {
})
</script>

<style>
.hh_id {
    max-width: 300px;

}
</style>