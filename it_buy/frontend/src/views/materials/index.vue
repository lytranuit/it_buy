<template>
  <div class="row clearfix">
    <div class="col-12">
      <h5 class="card-header drag-handle">
        <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2" @click="openNew"></Button>
        <Button label="Xóa" icon="pi pi-trash" class="p-button-danger p-button-sm" @click="confirmDeleteSelected"
          :disabled="!selectedProducts || !selectedProducts.length"></Button>
      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt"
            scrollHeight="70vh" v-model:selection="selectedProducts" :paginator="true"
            :rowsPerPageOptions="[10, 50, 100]" :rows="rows" :totalRecords="totalRecords" @page="onPage($event)"
            :rowHover="true" :loading="loading" responsiveLayout="scroll" :resizableColumns="true"
            columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu" editMode="cell"
            @cell-edit-complete="onCellEditComplete" @sort="onSort($event)">
            <template #header>
              <div style="width: 200px">
                <TreeSelect :options="columns" v-model="showing" multiple :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
                </TreeSelect>
              </div>
            </template>

            <template #empty>
              <div class="text-center">Không có dữ liệu.</div>
            </template>
            <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
            <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
              :class="col.className" :showFilterMatchModes="false" :sortable="col.sortable">
              <template #body="slotProps">
                <template v-if="col.data == 'mansx'">
                  ({{ slotProps.data.mansx }})
                  {{ slotProps.data.nhasanxuat?.tennsx }}
                </template>
                <template v-else-if="col.data == 'mancc'">
                  ({{ slotProps.data.mancc }})
                  {{ slotProps.data.nhacungcap?.tenncc }}
                </template>

                <template v-else-if="col.data == 'image_url'">
                  <div>
                    <a :href="VITE_BASEURL + slotProps.data.image_url + '?token=' + user.key_private" target="_blank">
                      <img :src="VITE_BASEURL + slotProps.data.image_url + '?token=' + user.key_private"
                        class="img-fluid" style="width: 100px;text-align: center;" />
                    </a>
                    <i class="pi pi-camera ml-2" style="cursor: pointer;" @click="uploadImage(slotProps.data.id)"></i>
                    <input type="file" accept="image/*" class="d-none" capture="environment"
                      :ref="(el) => (fileInput['fileInput-' + slotProps.data.id] = el)"
                      @change="onFileChange($event, slotProps.data.mahh)" @click="$event.stopPropagation()" />
                  </div>
                </template>
                <div v-else v-html="slotProps.data[col.data]"></div>
              </template>

              <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <template v-if="col.data == 'nhom'">
                  <Nhom v-model="filterModel.value" :multiple="false" @change="filterCallback()" class="p-column-filter"
                    style="width: 300px;"></Nhom>
                </template>
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()" v-else
                  class="p-column-filter" />
              </template>
            </Column>

            <Column style="width: 1rem">
              <template #body="slotProps">
                <a class="p-link text-warning mr-2 font-16" @click="editProduct(slotProps.data)">
                  <i class="pi pi-pencil"></i>
                </a>
                <a class="p-link text-danger font-16" @click="confirmDeleteProduct(slotProps.data)"
                  v-if="slotProps.data.nhom == 'Khac'">
                  <i class="pi pi-trash"></i>
                </a>
              </template>
            </Column>
          </DataTable>
        </div>
      </section>
    </div>

    <PopupAdd @save="loadLazyData" v-if="visibleDialog"></PopupAdd>
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

import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import Calendar from "primevue/calendar";

import { useToast } from "primevue/usetoast";
import materialApi from "../../api/materialApi";
import { useAuth } from "../../stores/auth";
import PopupAdd from "../../components/materials/PopupAdd.vue";
import { useMaterials } from "../../stores/materials";
import { storeToRefs } from "pinia";
import { RouterLink } from "vue-router";
import Nhom from "../../components/TreeSelect/Nhom.vue";
const store = useAuth();
const toast = useToast();

const { user } = storeToRefs(store);
const VITE_BASEURL = import.meta.env.VITE_BASEURL;
const fileInput = ref({});
const getfileInputRef = (id) => {
  return fileInput.value[id];
};
const uploadImage = (id) => {
  var input = getfileInputRef("fileInput-" + id);
  input.click();
};
const onFileChange = (event, mahh) => {
  const file = event.target.files[0];
  if (file) {
    console.log(file);
    const formData = new FormData();
    formData.append("file", file);
    formData.append("mahh", mahh);

    materialApi.uploadImage(formData).then((res) => {
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
  }
};
////Datatable
const sorts = ref({});
const datatable = ref();
const columns = ref([
  {
    id: 0,
    label: "Hình đại diện",
    data: "image_url",
    className: "text-center image_url",
  },
  {
    id: 1,
    label: "Mã hàng hóa",
    data: "mahh",
    className: "text-center",
    filter: true,
    sortable: true,
  },
  {
    id: 2,
    label: "Tên hàng hóa",
    data: "tenhh",
    className: "",
    filter: true,
    sortable: true,
  },
  {
    id: 3,
    label: "ĐVT",
    data: "dvt",
    className: "text-center",
  },
  {
    id: 4,
    label: "Nhóm",
    data: "nhom",
    className: "text-center",
    filter: true,
  },
  {
    id: 5,
    label: "Nhà sản xuất",
    data: "mansx",
    className: "text-center",
    filter: true,
  },
  {
    id: 6,
    label: "Nhà cung cấp",
    data: "mancc",
    className: "text-center",
    filter: true,
  },
]);
const filters = ref({
  mahh: { value: null, matchMode: FilterMatchMode.CONTAINS },
  tenhh: { value: null, matchMode: FilterMatchMode.CONTAINS },
  mancc: { value: null, matchMode: FilterMatchMode.CONTAINS },
  mansx: { value: null, matchMode: FilterMatchMode.CONTAINS },
  nhom: { value: null, matchMode: FilterMatchMode.CONTAINS },
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
  let data_sorts = {};
  // console.log(sorts.value);
  for (let key in sorts.value) {
    data_sorts[key] = sorts.value[key];
  }
  return {
    draw: draw.value,
    start: first.value,
    length: rows.value,
    sorts: data_sorts,
    filters: data_filters,
  };
});
const onSort = (event) => {
  // console.log(event);
  sorts.value = {};
  sorts.value[event.sortField] = event.sortOrder;
  loadLazyData();
};
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
// const model = ref();
const store_materials = useMaterials();
const { model, headerForm, visibleDialog } = storeToRefs(store_materials);
///Control
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

const openNew = () => {
  model.value = { nhom: 'Khac' };
  headerForm.value = "Tạo mới";
  visibleDialog.value = true;
};

const editProduct = (m) => {
  headerForm.value = m.mahh;
  model.value = { ...m };
  visibleDialog.value = true;
};
const confirmDeleteSelected = () => {
  deleteProductsDialog.value = true;
};
const confirmDeleteProduct = (m) => {
  model.value = m;
  deleteProductDialog.value = true;
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
  let list_no = selectedProducts.value.filter((val) => val.nhom != "Khac");
  if (list_no.length > 0) {
    alert("Không xóa được");
    return;
  }
  let list = selectedProducts.value.map((item) => {
    return item.mahh;
  });
  waiting.value = true;
  materialApi.remove({ item: list }).then((res) => {
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
<style>
.image_url {
  min-width: 150px;

}
</style>