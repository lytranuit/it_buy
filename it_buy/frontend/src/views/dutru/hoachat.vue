<template>
  <div class="row clearfix">
    <div class="col-12 mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }"
        :model="[{ label: 'Dự trù' }, { label: 'Hóa chất, thuốc thử QC' },]"></Breadcrumb>
    </div>
    <div class="col-12">
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
                  <RouterLink :to="'/dutru/edit/' + slotProps.data[col.data]">
                    <i class="fas fa-pencil-alt mr-2"></i>
                    {{ slotProps.data[col.data] }}
                  </RouterLink>
                </template>

                <template v-else-if="col.data == 'name'">
                  <div>
                    <div style="text-wrap: pretty;">
                      <RouterLink :to="'/dutru/edit/' + slotProps.data.id" class="text-blue">{{ slotProps.data.name }}
                      </RouterLink>
                    </div>
                    <small>Tạo bởi <i>{{ slotProps.data.user_created_by?.fullName }}</i> lúc {{
            formatDate(slotProps.data.created_at, "YYYY-MM-DD HH:mm") }}</small>
                  </div>
                </template>

                <template v-else-if="col.data == 'status_id'">
                  <div class="text-center">
                    <Button label="Hoàn thành" class="p-button-success" size="small"
                      v-if="slotProps.data['date_finish']"></Button>
                    <Button label="Nháp" class="p-button-secondary" size="small"
                      v-else-if="slotProps.data[col.data] == 1"></Button>
                    <Button label="Đang trình ký" class="p-button-warning" size="small"
                      v-else-if="slotProps.data[col.data] == 2"></Button>
                    <Button label="Chờ ký duyệt" class="p-button-warning" size="small"
                      v-else-if="slotProps.data[col.data] == 3"></Button>
                    <Button label="Đã duyệt" class="p-button-success" size="small"
                      v-else-if="slotProps.data[col.data] == 4"></Button>
                    <Button label="Không duyệt" class="p-button-danger" size="small"
                      v-else-if="slotProps.data[col.data] == 5"></Button>
                  </div>
                </template>
                <div v-else v-html="slotProps.data[col.data]"></div>
              </template>

              <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                  class="p-column-filter" />
              </template>
            </Column>
          </DataTable>
        </div>
      </section>
    </div>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import dutruApi from "../../api/dutruApi";
import Breadcrumb from 'primevue/breadcrumb';
import Button from "primevue/button";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column"; ////Datatable
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../components/Loading.vue";
import { formatDate } from "../../utilities/util";
const type_id = ref(3);
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
    label: "Mã",
    data: "code",
    className: "text-center",
    filter: true,
  },
  {
    id: 2,
    label: "Tên",
    data: "name",
    className: "text-center",
    filter: true,
  },
  {
    id: 3,
    label: "Trạng thái",
    data: "status_id",
    className: "text-center",
  },
]);
const filters = ref({
  id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  code: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_dutru"; ////
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
    type_id: type_id.value
  };
});
const dt = ref(null);

////Data table
const loadLazyData = () => {
  loading.value = true;
  dutruApi.table(lazyParams.value).then((res) => {
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
      dutruApi.delete(id).then((res) => {
        loadLazyData();
      });
    },
  });
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
const waiting = ref();
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>
