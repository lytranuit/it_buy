<template>
  <div id="TablemuahangChitiet">
    <DataTable showGridlines :value="modelncc.chitiet" ref="dt" class="p-datatable-ct" :rowHover="true"
      :loading="loading" responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand"
      v-model:selection="selected">
      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column v-for="col in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
        :showFilterMatchModes="false" :class="col.className">
        <template #body="slotProps">
          <template v-if="col.data == 'mahh'">
            <div class="d-flex align-items-center">
              <material-auto-complete v-model="slotProps.data[col.data]" @item-select="select($event, slotProps.data)"
                :disabled="readonly">
              </material-auto-complete>
            </div>
          </template>
          <template v-else-if="col.data == 'tenhh'">
            <InputText v-model="slotProps.data[col.data]" class="p-inputtext-sm" :disabled="readonly" />
          </template>
          <template v-else-if="col.data == 'nhasx'">
            <InputText v-model="slotProps.data[col.data]" class="p-inputtext-sm" :disabled="readonly" />
          </template>
          <template v-else-if="col.data == 'moq'">
            <InputText v-model="slotProps.data[col.data]" class="p-inputtext-sm" :disabled="readonly" />
          </template>
          <template v-else-if="col.data == 'dvt'">
            {{ slotProps.data["dvt"] }}
            <i class="fas fa-sync-alt" style="cursor: pointer" @click="openOp($event, slotProps.data)"></i></template>
          <template v-else-if="col.data == 'soluong'">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" @update:modelValue="changeDongia()"
              :disabled="readonly" :inputStyle="{ width: '100px' }" :maxFractionDigits="5" />
          </template>
          <template v-else-if="col.data == 'dongia'">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :suffix="' ' + modelncc.tiente"
              @update:modelValue="changeDongia()" :disabled="readonly" :maxFractionDigits="5"
              :inputStyle="{ width: '150px' }" />
          </template>
          <template v-else-if="col.data == 'vat'">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :suffix="'%'"
              :inputStyle="{ width: '50px' }" @update:modelValue="changeDongia()" :disabled="readonly"
              :maxFractionDigits="0" />
          </template>

          <template v-else-if="col.data == 'thanhtien' || col.data == 'thanhtien_vat'">
            <InputNumber v-model="slotProps.data[col.data]" class="p-inputtext-sm" :suffix="' ' + modelncc.tiente"
              :disabled="readonly || !modelncc.is_edit" :maxFractionDigits="2" :inputStyle="{ width: '150px' }" />
          </template>

          <template v-else>
            {{ slotProps.data[col.data] }}
          </template>
        </template>
      </Column>
    </DataTable>
    <OverlayPanel ref="op">
      <div>
        Qui đổi từ {{ editingRow.soluong }}
        <input class="form-control form-control-sm d-inline-block" style="width: 60px; margin-right: 5px"
          v-model="editingRow.dvt" />
        mua hàng =
        <input class="form-control form-control-sm d-inline-block" style="width: 60px; margin-right: 5px"
          v-model="editingRow.soluong_quidoi" @change="changeQuidoi(editingRow)" />
        <b>{{ editingRow.dvt_dutru }}</b> dự trù (Tỉ lệ 1:{{
          editingRow.quidoi
        }})
      </div>
    </OverlayPanel>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import InputNumber from "primevue/inputnumber";
import { storeToRefs } from "pinia";

import MaterialAutoComplete from "../AutoComplete/MaterialAutoComplete.vue";
import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../utilities/rand";
import { formatPrice } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { useGeneral } from "../../stores/general";
import InputText from "primevue/inputtext";
import OverlayPanel from "primevue/overlaypanel";

const readonly = ref(false);
const store_muahang = useMuahang();
const store_general = useGeneral();
const { nccs, nccs_chitiet, model } = storeToRefs(store_muahang);
const { materials } = storeToRefs(store_general);
const confirm = useConfirm();

const editingRow = ref({});
const loading = ref(false);
const selected = ref();
const modelncc = computed(() => {
  return nccs.value[props.index];
});
const is_vat = computed(() => {
  return modelncc.value ? modelncc.value.is_vat : false;
});
const columns = ref([]);

const op = ref();
const openOp = (event, row) => {
  console.log(event);
  console.log(op.value);
  editingRow.value = row;
  op.value.toggle(event);
};
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
const changeQuidoi = (row) => {
  if (row.soluong_quidoi) {
    row.quidoi = row.soluong_quidoi / row.soluong;
  }
};
const props = defineProps({
  index: Number,
});

const select = (event, row) => {
  //   console.log(event);
  var id = event.value.id;
  row.mahh = id;
  // store_general.changeMaterial(row);
};
onMounted(async () => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
  // await store_general.fetchMaterials();
  // console.log(selectedColumns);
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
          data: "mahh",
          className: "text-center mahh",
        },
        {
          label: "Tên",
          data: "tenhh",
          className: "text-center",
        },
        {
          label: "Nhà sản xuất",
          data: "nhasx",
          className: "text-center",
        },
        {
          label: "ĐVT",
          data: "dvt",
          className: "text-center",
        },
        {
          label: "MOQ",
          data: "moq",
          className: "text-center moq",
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
          data: "mahh",
          className: "text-center mahh",
        },
        {
          label: "Tên",
          data: "tenhh",
          className: "text-center",
        },
        {
          label: "Nhà sản xuất",
          data: "nhasx",
          className: "text-center",
        },
        {
          label: "ĐVT",
          data: "dvt",
          className: "text-center",
        }, {
          label: "MOQ",
          data: "moq",
          className: "text-center moq",
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
defineExpose({
  changeDongia,
});
</script>

<style>
.mahh,
.moq {
  min-width: 150px;
}
</style>