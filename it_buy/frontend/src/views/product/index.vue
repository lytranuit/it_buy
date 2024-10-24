<template>
  <div class="row clearfix">
    <Toast />
    <div class="col-12">
      <h5 class="card-header drag-handle">
        <Button
          label="Tạo mới"
          icon="pi pi-plus"
          class="p-button-success p-button-sm mr-2"
          @click="openNew"
        ></Button>
      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <DataTable
            class="p-datatable-customers"
            showGridlines
            :value="datatable"
            :lazy="true"
            ref="dt"
            scrollHeight="70vh"
            :paginator="true"
            :rowsPerPageOptions="[10, 50, 100]"
            :rows="rows"
            :totalRecords="totalRecords"
            @page="onPage($event)"
            :rowHover="true"
            :loading="loading"
            responsiveLayout="scroll"
            :resizableColumns="true"
            columnResizeMode="expand"
            v-model:filters="filters"
            filterDisplay="menu"
          >
            <template #header>
              <div style="width: 200px">
                <TreeSelect
                  :options="columns"
                  v-model="showing"
                  multiple
                  :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'"
                  :normalizer="normalizer"
                >
                </TreeSelect>
              </div>
            </template>

            <template #empty> Không có dữ liệu. </template>

            <Column style="width: 1rem">
              <template #body="slotProps">
                <a
                  class="p-link text-warning mr-2 font-16"
                  @click="editProduct(slotProps.data)"
                  ><i class="pi pi-pencil"></i
                ></a>
                <a
                  class="p-link text-danger font-16"
                  @click="confirmDelete(slotProps.data)"
                  ><i class="pi pi-trash"></i
                ></a>
              </template>
            </Column>
            <Column
              v-for="col of selectedColumns"
              :field="col.data"
              :header="col.label"
              :key="col.data"
              :class="col.data"
              :showFilterMatchModes="false"
            >
              <template #body="slotProps">
                <template v-if="col.data == 'ngaycapsodk'">
                  <div>{{ formatDate(slotProps.data[col.data]) }}</div>
                </template>
                <template v-else-if="col.data == 'ngayhethansodk'">
                  <div>{{ formatDate(slotProps.data[col.data]) }}</div>
                </template>
                <template v-else-if="col.data == 'tenhoatchat'">
                  <div style="white-space: break-spaces">
                    {{ slotProps.data[col.data] }}
                  </div>
                </template>
                <div v-html="slotProps.data[col.data]" v-else></div>
              </template>

              <template
                #filter="{ filterModel, filterCallback }"
                v-if="col.filter == true"
              >
                <InputText
                  type="text"
                  v-model="filterModel.value"
                  @keydown.enter="filterCallback()"
                  class="p-column-filter"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </section>
    </div>

    <PopupAdd @save="loadLazyData"></PopupAdd>

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
import productApi from "../../api/productApi";
import { useAuth } from "../../stores/auth";
import { useProduct } from "../../stores/product";
import PopupAdd from "../../components/product/PopupAdd.vue";
import { storeToRefs } from "pinia";
import { formatDate } from "../../utilities/util";
import { useConfirm } from "primevue/useconfirm";
import moment from "moment";
const toast = useToast();
const confirm = useConfirm();

const store = useAuth();
////Datatable
const datatable = ref();
const columns = ref([
  {
    label: "Mã thương mại",
    data: "mahh",
    className: "text-center",
    filter: true,
  },
  {
    label: "Tên thương mại",
    data: "tenhh",
    className: "text-center",
    filter: true,
  },
  {
    label: "Mã gốc",
    data: "mahh_goc",
    className: "text-center",
    filter: true,
  },
  {
    label: "Tên gốc",
    data: "tenhh_goc",
    className: "text-center",
    filter: true,
  },
  {
    label: "Khu vực",
    data: "mapl",
    className: "text-center",
    filter: true,
  },
  {
    label: "Tên hoạt chất",
    data: "tenhoatchat",
    className: "text-center",
    filter: true,
  },
  {
    label: "Dạng bào chế",
    data: "dangbaoche",
    className: "text-center",
    filter: true,
  },
  {
    label: "Qui cách",
    data: "quicachdonggoi",
    className: "text-center",
    filter: true,
  },
  {
    label: "Hạn dùng",
    data: "handung",
    className: "text-center",
    filter: true,
  },
  {
    label: "Số đăng ký",
    data: "sodk",
    className: "text-center",
    filter: true,
  },
  {
    label: "Ngày cấp SDK",
    data: "ngaycapsodk",
    className: "text-center",
  },
  {
    label: "Ngày hết hạn SDK",
    data: "ngayhethansodk",
    className: "text-center",
  },
  {
    label: "Ghi chú",
    data: "ghichu",
    className: "text-center",
    filter: true,
  },
]);
const filters = ref({});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_product"; ////
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
const selectedProducts = ref();
const selectedColumns = computed(() => {
  return columns.value.filter((col) => showing.value.includes(col.data));
});
const normalizer = (node) => {
  return {
    id: node.data,
    label: node.label,
  };
};
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
const dt = ref(null);

////Form
const store_product = useProduct();
const { model, headerForm, visibleDialog } = storeToRefs(store_product);
///Control
const productDialog = ref();
const deleteProductsDialog = ref();
const deleteProductDialog = ref();
const waiting = ref(false);

////Data table
const loadLazyData = () => {
  loading.value = true;
  productApi.table(lazyParams.value).then((res) => {
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
  // console.log(m);
  headerForm.value = m.mahh;
  model.value = { ...m };
  if (model.value.ngaycapsodk) {
    model.value.ngaycapsodk = moment(model.value.ngaycapsodk).toDate();
  }
  if (model.value.ngayhethansodk) {
    model.value.ngayhethansodk = moment(model.value.ngayhethansodk).toDate();
  }
  visibleDialog.value = true;
};

const confirmDelete = (m) => {
  confirm.require({
    message: "Bạn có muốn xóa row này?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      productApi.remove({ item: [m.id] }).then((res) => {
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
        loadLazyData();
      });
    },
  });
};
////Core
onMounted(() => {
  let cache = localStorage.getItem(column_cache);
  if (!cache) {
    showing.value = columns.value.map((item) => {
      return item.data;
    });
  } else {
    showing.value = JSON.parse(cache);
  }
  for (var col of columns.value) {
    if (!col.filter) continue;
    filters.value[col.data] = {
      value: null,
      matchMode: FilterMatchMode.CONTAINS,
    };
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
.tenhoatchat {
  min-width: 500px;
}
</style>