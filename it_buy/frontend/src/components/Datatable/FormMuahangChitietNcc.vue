<template>
  <div id="TablemuahangChitiet">
    <DataTable
      showGridlines
      :value="modelncc.chitiet"
      ref="dt"
      class="p-datatable-ct"
      :rowHover="true"
      :loading="loading"
      responsiveLayout="scroll"
      :resizableColumns="true"
      columnResizeMode="expand"
      v-model:selection="selected"
    >
      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column
        v-for="col in selectedColumns"
        :field="col.data"
        :header="col.label"
        :key="col.data"
        :showFilterMatchModes="false"
        :className="col.className"
      >
        <template #body="slotProps">
          <template v-if="col.data == 'hh_id'">
            {{ slotProps.data["mahh"] }}
          </template>
          <template v-else-if="col.data == 'tenhh' || col.data == 'dvt'">
            <InputText
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              :disabled="readonly"
            />
          </template>
          <template v-else-if="col.data == 'soluong'">
            <InputNumber
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              @update:modelValue="changeDongia()"
              :disabled="readonly"
              :inputStyle="{ width: '100px' }"
              :maxFractionDigits="5"
            />
          </template>
          <template v-else-if="col.data == 'dongia'">
            <InputNumber
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              :suffix="' ' + modelncc.tiente"
              @update:modelValue="changeDongia()"
              :disabled="readonly"
              :maxFractionDigits="5"
            />
          </template>
          <template v-else-if="col.data == 'vat'">
            <InputNumber
              v-model="slotProps.data[col.data]"
              class="p-inputtext-sm"
              :suffix="'%'"
              :inputStyle="{ width: '50px' }"
              @update:modelValue="changeDongia()"
              :disabled="readonly"
              :maxFractionDigits="0"
            />
          </template>

          <template
            v-else-if="col.data == 'thanhtien' || col.data == 'thanhtien_vat'"
          >
            {{ formatPrice(slotProps.data[col.data], 2) }} {{ modelncc.tiente }}
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
import { onMounted, ref, watch, computed } from "vue";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import InputNumber from "primevue/inputnumber";
import { storeToRefs } from "pinia";

import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../utilities/rand";
import { formatPrice } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { useGeneral } from "../../stores/general";
import InputText from "primevue/inputtext";

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
const is_vat = computed(() => {
  return modelncc.value ? modelncc.value.is_vat : false;
});
const columns = ref([
  {
    label: "STT",
    data: "stt",
    className: "text-center",
  },
  {
    label: "Mã",
    data: "hh_id",
    className: "text-center",
  },
  {
    label: "Tên",
    data: "tenhh",
    className: "text-center",
  },
  {
    label: "ĐVT",
    data: "dvt",
    className: "text-center",
  },
  {
    label: "Số lượng",
    data: "soluong",
    className: "text-center",
  },
  {
    label: "Đơn giá",
    data: "dongia",
    className: "text-center",
  },
  {
    label: "VAT",
    data: "vat",
    className: "text-center",
  },
  {
    label: "Thành tiền (Chưa vat)",
    data: "thanhtien",
    className: "text-center",
  },
  {
    label: "Thành tiền",
    data: "thanhtien_vat",
    className: "text-center",
  },
]);

const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const changeDongia = () => {
  var ncc = nccs.value[props.index];
  var thanhtien = 0;
  var thanhtien_vat = 0;
  var tienvat = 0;
  for (var item of ncc.chitiet) {
    item.dongia_vat = item.dongia + (item.dongia * item.vat) / 100;

    item.thanhtien = item.dongia * item.soluong || 0;
    item.thanhtien_vat = item.dongia_vat * item.soluong || 0;
    item.tienvat = item.thanhtien_vat - item.thanhtien;
    thanhtien += item.thanhtien;
    thanhtien_vat += item.thanhtien_vat;
    tienvat += item.tienvat;
  }
  ncc.thanhtien = thanhtien;
  ncc.thanhtien_vat = thanhtien_vat;
  ncc.tienvat = tienvat;
  var tonggiatri = ncc.thanhtien + ncc.tienvat + ncc.phigiaohang - ncc.ck;
  ncc.tonggiatri = tonggiatri;
};
const props = defineProps({
  index: Number,
});
onMounted(() => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
  // console.log(props.index)
  // console.log(modelncc.value)
});

watch(
  is_vat,
  () => {
    // console.log(is_vat);
    if (is_vat.value) {
      columns.value = [
        {
          label: "STT",
          data: "stt",
          className: "text-center",
        },
        {
          label: "Mã",
          data: "hh_id",
          className: "text-center",
        },
        {
          label: "Tên",
          data: "tenhh",
          className: "text-center",
        },
        {
          label: "ĐVT",
          data: "dvt",
          className: "text-center",
        },
        {
          label: "Số lượng",
          data: "soluong",
          className: "text-center",
        },
        {
          label: "Đơn giá",
          data: "dongia",
          className: "text-center",
        },
        {
          label: "VAT",
          data: "vat",
          className: "text-center",
        },
        {
          label: "Thành tiền (Chưa vat)",
          data: "thanhtien",
          className: "text-center",
        },
        {
          label: "Thành tiền",
          data: "thanhtien_vat",
          className: "text-center",
        },
      ];
    } else {
      columns.value = [
        {
          label: "STT",
          data: "stt",
          className: "text-center",
        },
        {
          label: "Mã",
          data: "hh_id",
          className: "text-center",
        },
        {
          label: "Tên",
          data: "tenhh",
          className: "text-center",
        },
        {
          label: "ĐVT",
          data: "dvt",
          className: "text-center",
        },
        {
          label: "Số lượng",
          data: "soluong",
          className: "text-center",
        },
        {
          label: "Đơn giá",
          data: "dongia",
          className: "text-center",
        },
        {
          label: "Thành tiền",
          data: "thanhtien_vat",
          className: "text-center",
        },
      ];
      ///// Đưa VAT = 0
      var ncc = nccs.value[props.index];
      if (ncc) {
        for (var item of ncc.chitiet) {
          item.vat = 0;
        }
        changeDongia();
      }
    }
  },
  { immediate: true }
);
</script>