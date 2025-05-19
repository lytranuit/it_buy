<template>
  <div id="TableNhapVTChitiet">
    <DataTable showGridlines :value="datatable" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
      responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
      <template #header>
        <div class="d-inline-flex" style="width: 200px">
          <Button label="Thêm" icon="pi pi-plus" class="p-button-success mr-2" @click="addRow" size="small"></Button>
          <Button label="Xóa" icon="pi pi-trash" class="p-button-danger" size="small"
            :disabled="!selected || !selected.length" @click="confirmDeleteSelected"></Button>
        </div>
        <div class="d-inline-flex float-right"></div>
      </template>

      <template #empty>
        <div class="text-center">Không có dữ liệu.</div>
      </template>
      <Column selectionMode="multiple"></Column>
      <Column v-for="col in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
        :showFilterMatchModes="false" :class="col.data">
        <template #body="slotProps">
          <template v-if="col.data == 'mahh'">
            <div class="d-flex align-items-center">
              <MaterialTreeSelect :multiple="false" v-model="slotProps.data[col.data]"
                @update:modelValue="changeMaterial(slotProps.data, $event)">
                <template #header>
                  <div class="d-flex justify-content-between align-items-center">
                    <span>Chọn hàng hóa</span>
                    <Button icon="pi pi-times" class="p-button-text" @click="$emit('update:modelValue', null)"></Button>
                  </div>
                </template>
              </MaterialTreeSelect>
            </div>
          </template>
          <template v-else-if="col.data == 'mancc'">
            <NccTreeSelect v-model="slotProps.data[col.data]" :multiple="false" :useID="false" />
          </template>

          <template v-else-if="col.data == 'soluong'">
            <input-number v-model="slotProps.data[col.data]" class="p-inputtext-sm" :maxFractionDigits="2"
              :suffix="' ' + slotProps.data.dvt" />
          </template>

          <template v-else-if="col.data == 'ghichu'">
            <textarea v-model="slotProps.data[col.data]" class="form-control" />
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
import Button from "primevue/button";
import InputSwitch from "primevue/inputswitch";
import InputNumber from "primevue/inputnumber";
import { storeToRefs } from "pinia";

import { useConfirm } from "primevue/useconfirm";
import { rand } from "../../../utilities/rand";
import { useAuth } from "../../../stores/auth";
import { useVattu } from "../../../stores/Vattu";
import { useGeneral } from "../../../stores/general";
import { download, formatDate } from "../../../utilities/util";
import { useToast } from "primevue/usetoast";
import MaterialTreeSelect from "../../TreeSelect/MaterialTreeSelect.vue";
import NccTreeSelect from "../../TreeSelect/NccTreeSelect.vue";
const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const toast = useToast();
const store_Vattu = useVattu();
const store_general = useGeneral();

const { datatable, model, list_delete } = storeToRefs(store_Vattu);
const { materials, products } = storeToRefs(store_general);
const confirm = useConfirm();

const loading = ref(false);
const selected = ref();
const columns = ref(
  [
    {
      label: "STT(*)",
      data: "stt",
      className: "text-center",
    },
    {
      label: "Hàng hóa(*)",
      data: "mahh",
      className: "text-center",
    },
    {
      label: "Nhà cung cấp(*)",
      data: "mancc",
      className: "text-center",
    },
    {
      label: "Số lượng(*)",
      data: "soluong",
      className: "text-center",
    },
    {
      label: "Mô tả",
      data: "ghichu",
      className: "text-center",
    },
  ]
);
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const addRow = () => {
  var index = datatable.value.length;
  if (selected.value && selected.value.length == 1) {
    index = selected.value[selected.value.length - 1].stt;
  }
  datatable.value.splice(index, 0, {
    ids: rand(),
    soluong: 1,
    dvt: '',
  });
  calculate_stt();
};
const calculate_stt = () => {
  datatable.value = datatable.value.map((item, index) => {
    item.stt = index + 1;
    return item;
  });
};
const confirmDeleteSelected = () => {
  confirm.require({
    message: "Bạn có chắc muốn xóa những dòng đã chọn?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      datatable.value = datatable.value.filter((item) => {
        return !selected.value.includes(item);
      });

      if (!list_delete.value) {
        list_delete.value = [];
      }
      for (var item of selected.value) {
        if (!item.ids) {
          list_delete.value.push(item);
        }
      }
      selected.value = [];
      // selected
    },
  });
};

const changeMaterial = (row) => {
  // console.log(materials);
  var material = materials.value.find((item) => item.mahh == row.mahh);
  // console.log(material);
  if (material) {
    row.dvt = material.dvt;
    row.tenhh = material.tenhh;
  }
};
onMounted(async () => {
  // if(items)
});
defineExpose({
  calculate_stt,
  changeMaterial
});
</script>

<style>
.mahh {
  min-width: 300px;
}

.ghichu {
  min-width: 300px;
}
</style>
