<template>
  <div id="TableXuatVTChitiet">
    <DataTable showGridlines :value="datatable" ref="dt" class="p-datatable-ct" :rowHover="true" :loading="loading"
      responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:selection="selected">
      <template #header>
        <div class="d-inline-flex" style="width: 200px">
          <!-- <Button label="Thêm" icon="pi pi-plus" class="p-button-success mr-2" @click="addRow" size="small"></Button> -->
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
        :showFilterMatchModes="false" :class="col.className">
        <template #body="slotProps">
          <template v-if="col.data == 'mahh'">
            <div class="d-flex align-items-center">
              <MaterialTreeSelect :multiple="false" v-model="slotProps.data[col.data]"
                @update:modelValue="changeMaterial(slotProps.data, $event)" :disabled="slotProps.data.kt_nhap">
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
            <NccTreeSelect v-model="slotProps.data[col.data]" :multiple="false" :useID="false"
              style="width: 100%; display: inline-block;vertical-align: middle;"
              @update:modelValue="changeSupplier(slotProps.data, $event)" :disabled="slotProps.data.kt_nhap" />

            <!-- <i class="fas fa-receipt mx-2" style="cursor: pointer" @click="view_tonkho(slotProps.data)"></i> -->

          </template>

          <template v-else-if="col.data == 'kt_xuat'">
            <div class="custom-control custom-switch switch-success" style="height: 36px">
              <input type="checkbox" :id="'kt_xuat_' + slotProps.index" class="custom-control-input"
                v-model="slotProps.data[col.data]" :disabled="!checkPermissionXuat || slotProps.data.kt_nhap"
                @change="onXacnhanXuat(slotProps.data)" />
              <label :for="'kt_xuat_' + slotProps.index" class="custom-control-label"></label>
            </div>
          </template>
          <template v-else-if="col.data == 'kt_nhap'">
            <div class="custom-control custom-switch switch-success" style="height: 36px">
              <input type="checkbox" :id="'kt_nhap_' + slotProps.index" class="custom-control-input"
                v-model="slotProps.data[col.data]" :disabled="true" @change="onXacnhanNhap(slotProps.data)" />
              <label :for="'kt_nhap_' + slotProps.index" class="custom-control-label"></label>
            </div>
          </template>
          <template v-else-if="col.data == 'soluong'">
            <input-number v-model="slotProps.data[col.data]" class="p-inputtext-sm" :maxFractionDigits="2"
              :suffix="' ' + slotProps.data.dvt" :max="slotProps.data.tonkho" :disabled="slotProps.data.kt_nhap" />
          </template>

          <template v-else-if="col.data == 'tonkho'">
            <span v-if="slotProps.data[col.data] > 0">{{ formatPrice(slotProps.data[col.data]) }} {{ slotProps.data.dvt
            }}</span>
          </template>
          <template v-else-if="col.data == 'ghichu'">
            <textarea v-model="slotProps.data[col.data]" class="form-control" :disabled="slotProps.data.kt_nhap" />
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
import { download, formatDate, formatPrice } from "../../../utilities/util";
import { useToast } from "primevue/usetoast";
import MaterialTreeSelect from "../../TreeSelect/MaterialTreeSelect.vue";
import vattuApi from "../../../api/vattuApi";
import NccTreeSelect from "../../TreeSelect/NccTreeSelect.vue";
const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const toast = useToast();
const store_Vattu = useVattu();
const store_general = useGeneral();

const { datatable, model, list_delete, start_event } = storeToRefs(store_Vattu);
const { materials, products } = storeToRefs(store_general);
const confirm = useConfirm();

const checkPermissionXuat = computed(() => {
  return user.value && user.value.warehouses_vt.indexOf(model.value.noidi) != -1;
});
const checkPermissionNhap = computed(() => {
  return user.value && user.value.warehouses_vt.indexOf(model.value.noiden) != -1;
});
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
      className: "text-center mahh",
    },
    {
      label: "Nhà cung cấp(*)",
      data: "mancc",
      className: "text-center mancc",
    },
    {
      label: "Số lượng(*)",
      data: "soluong",
      className: "text-center",
    },
    {
      label: "Tồn kho",
      data: "tonkho",
      className: "text-center",
    },
    {
      label: "Mô tả",
      data: "ghichu",
      className: "text-center ghichu",
    },
    {
      label: "Xác nhận xuất",
      data: "kt_xuat",
      className: "text-center",
    },
    {
      label: "Người xuất",
      data: "user_xuat",
      className: "text-center",
    },
    {
      label: "Xác nhận nhập",
      data: "kt_nhap",
      className: "text-center",
    },
    {
      label: "Người nhập",
      data: "user_nhap",
      className: "text-center",
    },
  ]
);
const selectedColumns = computed(() => {
  return columns.value.filter((col) => col.hide != true);
});
const getTonkho = async (row) => {
  if (!row.mahh) {
    // alert("Vui lòng nhập mã hàng hóa!");
    return false;
  }
  if (!model.value.noidi) {
    alert("Chưa chọn kho xuất!");
    return false;
  }
  if (!row.mancc) {
    // alert("Vui lòng nhập mã hàng hóa!");
    return false;
  }
  if (!start_event.value) {
    return false;
  }
  var res = await vattuApi.getTonkho({ mahh: row.mahh, makho: model.value.noidi, mancc: row.mancc });
  if (res) {
    row.tonkho = res.soluong;
  } else {
    row.tonkho = 0;
  }
};
const addRow = () => {
  var index = datatable.value.length;
  if (selected.value && selected.value.length == 1) {
    index = selected.value[selected.value.length - 1].stt;
  }
  datatable.value.splice(index, 0, {
    ids: rand(),
    dvt: '',
    kt_xuat: true,
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
  for (var item of selected.value) {
    if (item.kt_nhap) {
      alert("Không thể xóa hàng hóa đã xác nhận nhập!");
      return false;
    }
  }
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
const onXacnhanXuat = async (row) => {
  if (!checkPermissionXuat.value) {
    alert("Bạn không có quyền xuất hàng hóa này!");
    return false;
  }
  if (row.kt_xuat) {
    row.user_xuat = user.value.email;
  } else {
    row.user_xuat = null;
  }
};
const onXacnhanNhap = async (row) => {
  if (!checkPermissionNhap.value) {
    alert("Bạn không có quyền nhập hàng hóa này!");
    return false;
  }
  if (row.kt_nhap) {
    row.user_nhap = user.value.email;
  } else {
    row.user_nhap = null;
  }
};
const changeMaterial = async (row) => {

  var material = materials.value.find((item) => item.mahh == row.mahh);
  // console.log(material);
  if (material) {
    row.dvt = material.dvt;
    row.tenhh = material.tenhh;
  }
  getTonkho(row);

};
const changeSupplier = async (row) => {

  getTonkho(row);
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
.mahh,
.mancc {
  min-width: 300px;
}

.ghichu {
  min-width: 300px;
}
</style>
