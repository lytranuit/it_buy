<template>
  <div class="row clearfix">
    <div class="col-12">
      <h5 class="card-header drag-handle">
        <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2" @click="openNew"
          v-if="is_CungungNVL"></Button>

      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt"
            scrollHeight="70vh" v-model:selection="selectedProducts" :paginator="true"
            :rowsPerPageOptions="[10, 50, 100]" :rows="rows" :totalRecords="totalRecords" @page="onPage($event)"
            :rowHover="true" :loading="loading" responsiveLayout="scroll" :resizableColumns="true"
            columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu">
            <template #header>
              <div style="width: 200px">
                <TreeSelect :options="columns" v-model="showing" multiple :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
                </TreeSelect>
              </div>
            </template>

            <template #empty> Không có dữ liệu. </template>
            <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
              :showFilterMatchModes="false">

              <template #body="slotProps">
                <template v-if="col.data == 'id'">
                  <RouterLink :to="'/danhgianhacungcap/details/' + slotProps.data[col.data]">
                    <i class="fas fa-pencil-alt mr-2"></i>
                    {{ slotProps.data[col.data] }}
                  </RouterLink>
                </template>

                <template v-else-if="col.data == 'mancc'">
                  {{ slotProps.data.nhacungcap.mancc }} - {{ slotProps.data.nhacungcap.tenncc }}
                </template>

                <template v-else-if="col.data == 'mansx'">
                  {{ slotProps.data.nhasanxuat.mansx }} - {{ slotProps.data.nhasanxuat.tennsx }}
                </template>
                <template v-else-if="col.data == 'status_id'">
                  <div class="text-center">
                    <Button label="Nháp" class="p-button-secondary" size="small"
                      v-if="!slotProps.data['is_thongbao']"></Button>
                    <Button label="Đã chấp nhận" class="p-button-success" size="small"
                      v-else-if="slotProps.data['is_chapnhan']"></Button>
                    <Button label="Đang duyệt" class="p-button-warning" size="small" v-else></Button>
                  </div>
                </template>

                <template v-else-if="col.data == 'created_by'">
                  <div v-if="slotProps.data['user_created_by']" class="d-flex">
                    <Avatar :image="slotProps.data.user_created_by.image_url"
                      :title="slotProps.data.user_created_by.fullName" size="small" shape="circle" /> <span
                      class="align-self-center ml-2">{{
            slotProps.data.user_created_by.fullName }}</span>
                  </div>
                </template>

                <div v-else v-html="slotProps.data[col.data]"></div>
              </template>

              <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                  class="p-column-filter" />
              </template>
            </Column>
            <Column style="width: 1rem">

              <template #body="slotProps">
                <a class="p-link text-danger font-16" @click="confirmDelete(slotProps.data['id'])"
                  v-if="slotProps.data['created_by'] == user.id"><i class="pi pi-trash"></i></a>
              </template>
            </Column>
          </DataTable>
        </div>
      </section>
    </div>
    <PopupAdd @save="loadLazyData()"></PopupAdd>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import danhgianhacungcapApi from "../../api/danhgianhacungcapApi";
import Avatar from "primevue/avatar";
import Button from "primevue/button";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column"; ////Datatable
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../components/Loading.vue";
import PopupAdd from "../../components/danhgianhacungcap/PopupAdd.vue";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
import { storeToRefs } from "pinia";
import { useAuth } from "../../stores/auth";
const store_danhgianhacungcap = useDanhgianhacungcap();
const { waiting, model, headerForm, visibleDialog } = storeToRefs(store_danhgianhacungcap);
const store = useAuth();
const { is_CungungNVL, is_Qa, user } = storeToRefs(store);
const confirm = useConfirm();
const datatable = ref();
const columns = ref([
  {
    id: 0,
    label: "ID",
    data: "id",
    className: "text-center",
    filter: true,
  },
  {
    id: 1,
    label: "Nguyên liệu",
    data: "tenhh",
    className: "text-center",
    filter: true,
  },
  {
    id: 2,
    label: "Nhà sản xuất",
    data: "mansx",
    className: "text-center",
    filter: true,
  },
  {
    id: 3,
    label: "Nhà cung cấp",
    data: "mancc",
    className: "text-center",
    filter: true,
  },
  {
    id: 4,
    label: "Trạng thái",
    data: "status_id",
    className: "text-center",
    filter: true,
  },

  {
    id: 5,
    label: "Người tạo",
    data: "created_by",
    className: "text-center",
    filter: true,
  },
]);
const filters = ref({
  id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  material_id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  ncc_id: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_danhgianhacungcap"; ////
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
const dt = ref(null);

////Data table
const loadLazyData = () => {
  loading.value = true;
  danhgianhacungcapApi.table(lazyParams.value).then((res) => {
    // console.log(res);
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

const confirmDelete = (id) => {
  confirm.require({
    message: "Bạn có muốn xóa row này?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      danhgianhacungcapApi.delete(id).then((res) => {
        loadLazyData();
      });
    },
  });
};

const openNew = () => {
  model.value = {};
  headerForm.value = "Tạo mới";
  visibleDialog.value = true;
};

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
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>
