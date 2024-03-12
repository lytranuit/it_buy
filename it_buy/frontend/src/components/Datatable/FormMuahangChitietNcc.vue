<template>
    <div id="TablemuahangChitiet">
        <DataTable showGridlines :value="modelncc.chitiet" ref="dt" class="p-datatable-ct" :rowHover="true"
            :loading="loading" responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand"
            v-model:selection="selected">

            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false">

                <template #body="slotProps">
                    <template v-if="col.data == 'hh_id'">
                        {{ slotProps.data["mahh"] }}
                    </template>

                    <template v-else-if="col.data == 'dongia'">
                        <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" suffix=" VND"
                            @update:modelValue="changeDongia()" :readonly="readonly"/>
                    </template>

                    <template v-else-if="col.data == 'thanhtien'">
                        {{ formatPrice(slotProps.data[col.data], 0) }} VND
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

import { useConfirm } from "primevue/useconfirm";
import { rand } from '../../utilities/rand'
import { formatPrice } from '../../utilities/util'
import { useMuahang } from '../../stores/muahang';
import { useGeneral } from '../../stores/general';

const readonly = ref(false);
const store_muahang = useMuahang();
const store_general = useGeneral();
const { nccs, nccs_chitiet, model } = storeToRefs(store_muahang);
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const modelncc = computed(() => {
    return nccs.value[props.index];
});
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
        label: "Đơn giá(*)",
        "data": "dongia",
        "className": "text-center"
    },
    {
        label: "Thành tiền(*)",
        "data": "thanhtien",
        "className": "text-center"
    }
])

const selectedColumns = computed(() => {
    return columns.value.filter(col => col.hide != true);
});
const changeDongia = () => {
    var ncc = nccs.value[props.index];
    var tonggiatri = 0;
    for (var item of ncc.chitiet) {
        item.thanhtien = (item.dongia * item.soluong) || 0;
        tonggiatri += item.thanhtien;
    }
    ncc.tonggiatri = tonggiatri;
}
const props = defineProps({
    index: Number,
})
onMounted(() => {

    if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
        readonly.value = true;
    }
    // console.log(props.index)
    // console.log(modelncc.value)
})
</script>