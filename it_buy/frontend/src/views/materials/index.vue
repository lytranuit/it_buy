<template>
  <div class="row clearfix">
    <Toast />
    <div class="col-12">
      <h5 class="card-header drag-handle">
        <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2" @click="openNew"></Button>
        <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm" @click="confirmDeleteSelected"
          :disabled="!selectedProducts || !selectedProducts.length"></Button>
      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt"
            scrollHeight="70vh" v-model:selection="selectedProducts" :paginator="true" :rowsPerPageOptions="[10, 50, 100]"
            :rows="rows" :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters"
            filterDisplay="menu" editMode="cell" @cell-edit-complete="onCellEditComplete">
            <template #header>
              <div style="width: 200px">
                <treeselect :options="columns" v-model="showing" multiple :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
                </treeselect>
              </div>
            </template>

            <template #empty>
              <div class="text-center">Không có dữ liệu.</div>
            </template>
            <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
            <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
              :showFilterMatchModes="false">
              <template #body="slotProps">
                <div v-if="slotProps.data[col.data] &&
                  (col.data == 'ngaysodk' ||
                    col.data == 'ngaythietke' ||
                    col.data == 'ngayhethanthietke')
                  ">
                  {{ moment(slotProps.data[col.data]).format("YYYY-MM-DD") }}
                </div>
                <div v-html="slotProps.data[col.data]" v-else></div>
              </template>
              <template #editor="{ data, field }" v-if="col.data == 'quidoi'">
                <input v-model="data[field]" class="form-control" />
              </template>
              <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                  class="p-column-filter" />
              </template>
            </Column>

            <Column style="width: 1rem">
              <template #body="slotProps">
                <a class="p-link text-warning mr-2 font-16" @click="editProduct(slotProps.data)"><i
                    class="pi pi-pencil"></i></a>
                <a class="p-link text-danger font-16" @click="confirmDeleteProduct(slotProps.data)"><i
                    class="pi pi-trash"></i></a>
              </template>
            </Column>
          </DataTable>
        </div>
      </section>
    </div>

    <Dialog v-model:visible="productDialog" :header="headerForm" :modal="true" class="p-fluid">
      <div class="row mb-2">
        <div class="field col">
          <label for="name">Mã hàng hóa <span class="text-danger">*</span></label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.mahh" required="true"
            :class="{ 'p-invalid': submitted && !model.mahh }" />
          <small class="p-error" v-if="submitted && !model.mahh">Required.</small>
        </div>
        <div class="field col">
          <label for="name">Tên hàng hóa <span class="text-danger">*</span></label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.tenhh" required="true"
            :class="{ 'p-invalid': submitted && !model.tenhh }" />
          <small class="p-error" v-if="submitted && !model.tenhh">Required.</small>
        </div>
        <div class="field col">
          <label for="name">ĐVT <span class="text-danger">*</span></label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.dvt" required="true"
            :class="{ 'p-invalid': submitted && !model.dvt }" />
          <small class="p-error" v-if="submitted && !model.dvt">Required.</small>
        </div>
      </div>
      <div class="row mb-2">
        <div class="field col">
          <label for="name">Nhà sản xuất</label>
          <Nsx v-model="model.mansx"></Nsx>
        </div>
        <div class="field col">
          <label for="name">Nhà cung cấp</label>
          <Ncc v-model="model.mancc"></Ncc>
        </div>
        <div class="field col">
          <label for="name">Qui đổi</label>
          <InputText class="p-inputtext-sm" v-model.trim="model.quidoi" type="number" />
        </div>
      </div>
      <div class="row mb-2">
        <div class="field col">
          <label for="name">Tên NVL của nhãn gốc</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.tennvlgoc" />
        </div>
        <div class="field col">
          <label for="name">Số đăng ký</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.masodk" />
        </div>
        <div class="field col">
          <label for="name">Ngày hết hạn đăng ký</label>
          <Calendar dateFormat="yy-mm-dd" inputClass="p-inputtext-sm" inputId="icon" v-model="model.ngaysodk" />
        </div>
        <div class="field col">
          <label for="name">Ghi chú</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.ghichu_sodk" />
        </div>
        <div class="field col">
          <label for="name">Mã nhóm</label>
          <InputText class="p-inputtext-sm" v-model.trim="model.manhom" />
        </div>
      </div>
      <div class="row mb-2">
        <div class="field col">
          <label for="name">Mã số thiết kế mới</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.masothietke" />
        </div>
        <div class="field col">
          <label for="name">Ngày hiệu lực mã số thiết kế mới</label>
          <Calendar dateFormat="yy-mm-dd" inputClass="p-inputtext-sm" inputId="icon" v-model="model.ngaythietke" />
        </div>
        <div class="field col">
          <label for="name">Mã số thiết kế cũ</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.masothietkecu" />
        </div>
        <div class="field col">
          <label for="name">Ngày hết hạn mã số thiết kế cũ</label>
          <Calendar dateFormat="yy-mm-dd" inputClass="p-inputtext-sm" inputId="icon" v-model="model.ngayhethanthietke" />
        </div>
        <div class="field col">
          <label for="name">Ghi chú thiết kế</label>
          <InputText id="name" class="p-inputtext-sm" v-model.trim="model.ghichu_thietke" />
        </div>
      </div>
      <template #footer>
        <Button label="Cancel" icon="pi pi-times" class="p-button-text" @click="hideDialog"></Button>
        <Button label="Save" icon="pi pi-check" class="p-button-text" @click="saveProduct"></Button>
      </template>
    </Dialog>

    <Dialog v-model:visible="deleteProductDialog" header="Xác nhận" :modal="true">
      <div class="confirmation-content">
        <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
        <span v-if="model">Bạn có muốn xóa <b>{{ model.mahh }}</b> này không?</span>
      </div>
      <template #footer>
        <Button label="Không" icon="pi pi-times" class="p-button-text" @click="deleteProductDialog = false"></Button>
        <Button label="Đồng ý" icon="pi pi-check" class="p-button-text" @click="deleteProduct"></Button>
      </template>
    </Dialog>

    <Dialog v-model:visible="deleteProductsDialog" header="Xác nhận" :modal="true">
      <div class="confirmation-content">
        <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem"></i>
        <span>Bạn có muốn xóa tất cả những mục đã chọn không?</span>
      </div>
      <template #footer>
        <Button label="Không" icon="pi pi-times" class="p-button-text" @click="deleteProductsDialog = false"></Button>
        <Button label="Đồng ý" icon="pi pi-check" class="p-button-text" @click="deleteSelectedProducts"></Button>
      </template>
    </Dialog>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from "vue";
import moment from "moment";
import Loading from "../../components/Loading.vue";
import Treeselect from "vue3-acies-treeselect";
import { useQLSX } from "../../stores/qlsx";
// import the styles
import "vue3-acies-treeselect/dist/vue3-treeselect.css";
// import the component

import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import Calendar from "primevue/calendar";
import Ncc from "../../components/TreeSelect/Ncc.vue";
import Nsx from "../../components/TreeSelect/Nsx.vue";

import Toast from "primevue/toast";
import { useToast } from "primevue/usetoast";
import materialApi from "../../api/materialApi";
import { useAuth } from "../../stores/auth";
const store = useAuth();
const toast = useToast();
const store_qlsx = useQLSX();
////Datatable
const datatable = ref();
const columns = ref([
  {
    id: 0,
    label: "Mã NVL",
    data: "mahh",
    className: "text-center",
    filter: true,
  },
  {
    id: 1,
    label: "Tên NVL",
    data: "tenhh",
    className: "text-center",
    filter: true,
  },
  {
    id: 2,
    label: "ĐVT",
    data: "dvt",
    className: "text-center",
  },
  {
    id: 3,
    label: "Mã NSX",
    data: "mansx",
    className: "text-center",
  },
  {
    id: 4,
    label: "Mã NCC",
    data: "mancc",
    className: "text-center",
  },
  {
    id: 5,
    label: "Tên NVL của nhãn gốc",
    data: "tennvlgoc",
    className: "text-center",
  },
  {
    id: 6,
    label: "Số đăng ký",
    data: "masodk",
    className: "text-center",
  },
  {
    id: 7,
    label: "Ngày hết hạn đăng ký",
    data: "ngaysodk",
    className: "text-center",
  },
  {
    id: 8,
    label: "Ghi chú số",
    data: "ghichu_sodk",
    className: "text-center",
  },
  {
    id: 9,
    label: "Mã số thiết kế",
    data: "masothietke",
    className: "text-center",
  },
  {
    id: 10,
    label: "Ngày hiệu lực thiết kế",
    data: "ngaythietke",
    className: "text-center",
  },
  {
    id: 11,
    label: "Ngày hết hạn thiết kế",
    data: "ngayhethanthietke",
    className: "text-center",
  },
  {
    id: 12,
    label: "Ghi chú thiết kế",
    data: "ghichu_thietke",
    className: "text-center",
  },
  {
    id: 13,
    label: "Qui đổi",
    data: "quidoi",
    className: "text-center",
  },
]);
const filters = ref({
  mahh: { value: null, matchMode: FilterMatchMode.CONTAINS },
  tenhh: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_materials"; ////
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
const selectedProducts = ref();
const selectedColumns = computed(() => {
  return columns.value.filter((col) => showing.value.includes(col.id));
});
const lazyParams = computed(() => {
  let data_filters = {};
  for (let key in filters.value) {
    data_filters[key] = filters.value[key].value;
  }
  return {
    draw: draw.value,
    start: first.value,
    length: rows.value,
    filters: data_filters,
  };
});

const onCellEditComplete = (event) => {
  let { newValue, value, newData, index } = event;
  if (newValue == value) {
    return false;
  }
  newData.old_key = newData.mahh;
  // console.log(event)
  datatable.value[index] = newData;
  materialApi.save(newData).then((res) => {
    if (res.success) {
      toast.add({
        severity: "success",
        summary: "Thành công",
        detail: "Thay đổi thành công",
        life: 3000,
      });
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    loadLazyData();
  });
};
const dt = ref(null);

////Form
const model = ref();
const old_key = ref();
const submitted = ref();
const headerForm = ref("");
///Control
const productDialog = ref();
const deleteProductsDialog = ref();
const deleteProductDialog = ref();
const waiting = ref(false);

////Data table
const loadLazyData = () => {
  loading.value = true;
  materialApi.table(lazyParams.value).then((res) => {
    // console.log(res);
    if (!res.data) store.logout();
    datatable.value = res.data;
    totalRecords.value = res.recordsFiltered;
    loading.value = false;
  });
};
const onPage = (event) => {
  first.value = event.first;
  rows.value = event.rows;
  draw.value = draw.value + 1;
  loadLazyData();
};

///Form
const valid = () => {
  if (!model.value.mahh.trim()) return false;
  if (!model.value.tenhh.trim()) return false;
  if (!model.value.dvt.trim()) return false;
  return true;
};
const saveProduct = () => {
  submitted.value = true;
  if (!valid()) return;

  if (model.value.ngaythietke instanceof Date) {
    model.value.ngaythietke = moment(model.value.ngaythietke).format(
      "YYYY-MM-DD"
    );
  }
  if (model.value.ngaysodk instanceof Date) {
    model.value.ngaysodk = moment(model.value.ngaysodk).format("YYYY-MM-DD");
  }
  if (model.value.ngayhethanthietke instanceof Date) {
    model.value.ngayhethanthietke = moment(
      model.value.ngayhethanthietke
    ).format("YYYY-MM-DD");
  }
  waiting.value = true;
  model.value.old_key = old_key.value;
  materialApi.save(model.value).then((res) => {
    waiting.value = false;
    if (res.success) {
      if (model.value.old_key != null) {
        //edit
        toast.add({
          severity: "success",
          summary: "Thành công",
          detail: "Cập nhật " + model.value.mahh + " thành công",
          life: 3000,
        });
      } else {
        toast.add({
          severity: "success",
          summary: "Thành công",
          detail: "Tạo mới thành công",
          life: 3000,
        });
      }
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    loadLazyData();
    model.value = {};
    productDialog.value = false;
  });
};
const openNew = () => {
  model.value = {};
  old_key.value = null;
  headerForm.value = "Tạo mới";
  submitted.value = false;
  productDialog.value = true;
};
const editProduct = (m) => {
  old_key.value = m.mahh;
  headerForm.value = m.mahh;
  model.value = { ...m };
  productDialog.value = true;
};
const confirmDeleteSelected = () => {
  deleteProductsDialog.value = true;
};
const confirmDeleteProduct = (m) => {
  model.value = m;
  deleteProductDialog.value = true;
};
const hideDialog = () => {
  productDialog.value = false;
  submitted.value = false;
};
const deleteProduct = () => {
  waiting.value = true;
  materialApi.remove({ item: [model.value.mahh] }).then((res) => {
    waiting.value = false;
    if (res.success) {
      toast.add({
        severity: "success",
        summary: "Thành công",
        detail: "Đã xóa thành công!",
        life: 3000,
      });
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    deleteProductDialog.value = false;
    model.value = {};
    loadLazyData();
  });
};
const deleteSelectedProducts = () => {
  // datatable.value = datatable.value.filter(val => !selectedProducts.value.includes(val));
  let list_mahh = selectedProducts.value.map((item) => {
    return item.mahh;
  });
  waiting.value = true;
  materialApi.remove({ item: list_mahh }).then((res) => {
    waiting.value = false;
    if (res.success) {
      toast.add({
        severity: "success",
        summary: "Thành công",
        detail: "Đã xóa thành công!",
        life: 3000,
      });
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    deleteProductsDialog.value = false;
    selectedProducts.value = null;
    loadLazyData();
  });
};

////Core
onMounted(() => {
  let cache = localStorage.getItem(column_cache);
  if (!cache) {
    showing.value = columns.value.map((item) => {
      return item.id;
    });
  } else {
    showing.value = JSON.parse(cache);
  }
  store_qlsx.fetchNhasx();
  store_qlsx.fetchNhacc();
  loadLazyData();
});
watch(showing, async (newa, old) => {
  localStorage.setItem(column_cache, JSON.stringify(newa));
});
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>
