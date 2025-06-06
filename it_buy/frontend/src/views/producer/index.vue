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
            scrollHeight="70vh" v-model:selection="selectedProducts" :paginator="true"
            :rowsPerPageOptions="[10, 50, 100]" :rows="rows" :totalRecords="totalRecords" @page="onPage($event)"
            :rowHover="true" :loading="loading" responsiveLayout="scroll" :resizableColumns="true"
            columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu" @sort="onSort($event)">
            <template #header>
              <div style="width: 200px">
                <TreeSelect :options="columns" v-model="showing" multiple :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
                </TreeSelect>
              </div>
            </template>

            <template #empty> Không có dữ liệu. </template>
            <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
            <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
              :showFilterMatchModes="false" :sortable="col.sortable">

              <template #body="slotProps">
                <div v-html="slotProps.data[col.data]"></div>
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

    <PopupAdd @save="loadLazyData"></PopupAdd>

    <Dialog v-model:visible="deleteProductDialog" header="Xác nhận" :modal="true">
      <div class="confirmation-content">
        <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
        <span v-if="model">Bạn có muốn xóa <b>{{ model.mansx }}</b> này không?</span>
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
import Loading from "../../components/Loading.vue";
// import the component

import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column";
import Button from "primevue/button";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";

import Toast from "primevue/toast";
import { useToast } from "primevue/usetoast";
import producerApi from "../../api/producerApi";
import { useAuth } from "../../stores/auth";
import { useProducer } from "../../stores/producer";
import PopupAdd from "../../components/producer/PopupAdd.vue";
import { storeToRefs } from "pinia";
const toast = useToast();

const store = useAuth();
////Datatable
const sorts = ref({});
const datatable = ref();
const columns = ref([
  {
    id: 0,
    label: "Mã",
    data: "mansx",
    className: "text-center",
    filter: true,
    sortable: true,
  },
  {
    id: 1,
    label: "Tên",
    data: "tennsx",
    className: "text-center",
    filter: true,
    sortable: true,
  },
]);
const filters = ref({
  mansx: { value: null, matchMode: FilterMatchMode.CONTAINS },
  tennsx: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_producer"; ////
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
const dt = ref(null);

////Form
const store_producer = useProducer();
const { model, headerForm, visibleDialog } = storeToRefs(store_producer);
///Control
const productDialog = ref();
const deleteProductsDialog = ref();
const deleteProductDialog = ref();
const waiting = ref(false);

////Data table
const loadLazyData = () => {
  loading.value = true;
  producerApi.table(lazyParams.value).then((res) => {
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
const openNew = () => {
  model.value = {};
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
const hideDialog = () => {
  productDialog.value = false;
  submitted.value = false;
};
const deleteProduct = () => {
  waiting.value = true;
  producerApi.remove({ item: [model.value.id] }).then((res) => {
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
  let items = selectedProducts.value.map((item) => {
    return item.id;
  });
  waiting.value = true;
  producerApi.remove({ item: items }).then((res) => {
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
