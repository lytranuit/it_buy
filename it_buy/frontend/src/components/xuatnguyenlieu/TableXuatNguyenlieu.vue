<template>
  <DataTable class="dt-responsive-table" showGridlines :value="datatable" :lazy="true" ref="dt" scrollHeight="70vh"
    v-model:selection="selectedProducts" :paginator="true" :rowsPerPageOptions="[10, 50, 100]" :rows="rows"
    :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading" responsiveLayout="scroll"
    :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu">
    <template #header>
      <div class="d-md-flex align-items-center">
        <div style="width: 200px">
          <TreeSelect :options="columns" v-model="showing" multiple :limit="0" :normalizer="normalizer"
            :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
          </TreeSelect>
        </div>
        <div class="ml-auto">
        </div>
        <div class="ml-3">
        </div>
      </div>
    </template>

    <template #empty> Không có dữ liệu. </template>
    <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
      :showFilterMatchModes="false">
      <template #body="slotProps">
        <span class="ui-column-title">{{ col.label }}</span>
        <div class="ui-column-data">
          <template v-if="col.data == 'id'">
            <router-link :to="'/xuatnguyenlieu/edit/' + slotProps.data[col.data]">
              <i class="fas fa-pencil-alt mr-2"></i>
              {{ slotProps.data[col.data] }}
            </router-link>
          </template>

          <template v-else-if="col.data == 'code'">
            <div>
              <div style="text-wrap: pretty">
                <router-link :to="'/xuatnguyenlieu/edit/' + slotProps.data.id" class="text-blue">{{ slotProps.data.code
                }}
                </router-link>
              </div>
              <small>Tạo bởi
                <i>{{ slotProps.data.created_by }}</i> lúc
                {{
                  formatDate(
                    slotProps.data.created_at,
                    "YYYY-MM-DD HH:mm"
                  )
                }}</small>
            </div>
          </template>
          <template v-else-if="col.data == 'date'">
            {{ formatDate(slotProps.data[col.data]) }}
          </template>
          <template v-else-if="col.data == 'bophan_id'">
            {{ bophan(slotProps.data.bophan_id) }}
          </template>
          <template v-else-if="col.data == 'status_id'">
            <div class="text-center">
              <Tag value="Hoàn thành" severity="success" size="small" v-if="slotProps.data['date_finish']" />
              <Tag value="Nháp" severity="secondary" size="small" v-else-if="slotProps.data[col.data] == 1" />
              <Tag value="Đang trình ký" severity="warning" size="small" v-else-if="slotProps.data[col.data] == 2" />
              <Tag value="Chờ ký duyệt" severity="warning" size="small" v-else-if="slotProps.data[col.data] == 3" />
              <Tag value="Đã duyệt" severity="success" size="small" v-else-if="slotProps.data[col.data] == 4" />
              <Tag value="Không duyệt" severity="danger" size="small" v-else-if="slotProps.data[col.data] == 5" />
            </div>
          </template>

          <div v-else v-html="slotProps.data[col.data]"></div>
        </div>
      </template>

      <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
        <template v-if="col.data == 'priority_id'">
          <select class="form-control" v-model="filterModel.value" @change="filterCallback()">
            <option value="1">Bình thường</option>
            <option value="2">Ưu tiên</option>
            <option value="3">Gấp</option>
          </select>
        </template>
        <template v-else-if="col.data == 'status_id'">
          <select class="form-control" v-model="filterModel.value" @change="filterCallback()">
            <option value="1">Nháp</option>
            <option value="2">Đang trình ký</option>
            <option value="3">Chờ ký duyệt</option>
            <option value="4">Đã duyệt</option>
            <option value="5">Không duyệt</option>
          </select>
        </template>
        <div v-else-if="col.data == 'bophan_id'" style="width: 200px">
          <DepartmentOfUserTreeSelect v-model="filterModel.value" @update:model-value="filterCallback()">
          </DepartmentOfUserTreeSelect>
        </div>
        <template v-else>
          <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
            class="p-column-filter" />
        </template>
      </template>
    </Column>
    <Column style="width: 1rem">
      <template #body="slotProps">
        <a class="p-link text-danger font-16" @click="confirmDelete(slotProps.data['id'])"
          v-if="slotProps.data.created_by == user.email"><i class="pi pi-trash"></i></a>
      </template>
    </Column>
  </DataTable>
  <Loading :waiting="waiting"></Loading>
</template>
<script setup>
import { onMounted, ref, computed, watch } from "vue";
import xuatnguyenlieuApi from "../../api/xuatnguyenlieuApi";
import Tag from "primevue/tag";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column"; ////Datatable
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../components/Loading.vue";
import { formatDate } from "../../utilities/util";
import SelectButton from "primevue/selectbutton";
import { useAuth } from "../../stores/auth";
import { storeToRefs } from "pinia";
import DepartmentOfUserTreeSelect from "../TreeSelect/DepartmentOfUserTreeSelect.vue";
import Api from "../../api/Api";
import { useRouter } from "vue-router";
const store = useAuth();
const {
  user,
} = storeToRefs(store);
// const props = defineProps({
//   type: Number
// })
const confirm = useConfirm();
const datatable = ref();
const list_filterTable = ref([{ label: "Tôi tạo", value: 1 }]);
const list_filterTable1 = ref([]);
const filterTable = ref(1);
const filterTable1 = ref();
const columns = ref([
  {
    label: "ID",
    data: "id",
    className: "text-center",
    filter: true,
  },

  {
    label: "Mã",
    data: "code",
    className: "text-center",
    filter: true,
  },
  {
    label: "Bộ phận đề nghị",
    data: "bophan_id",
    className: "text-center",
    filter: true,
  },
  {
    label: "Ngày mong muốn xuất",
    data: "date",
    className: "text-center",
  },
  {
    label: "Trạng thái",
    data: "status_id",
    className: "text-center",
    filter: true,
  },
  {
    label: "Ghi chú",
    data: "note",
    className: "text-center",
  },
]);
const filters = ref({
});
const router = useRouter();
const departments = ref([]);
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_xuatnguyenlieu"; ////
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
const selectedProducts = ref();
const selectedColumns = computed(() => {
  return columns.value.filter((col) => showing.value.includes(col.data));
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
    type_id: filterTable1.value,
    type1: filterTable.value,
  };
});
const dt = ref(null);

const normalizer = (node) => {
  return {
    id: node.data,
    label: node.label,
  };
};
////Data table
const loadLazyData = () => {
  loading.value = true;
  xuatnguyenlieuApi.table(lazyParams.value).then((res) => {
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
      xuatnguyenlieuApi.delete(id).then((res) => {
        loadLazyData();
      });
    },
  });
};

const confirmCopy = (id) => {
  confirm.require({
    message: "Bạn có muốn copy row này?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      router.push("/xuatnguyenlieu/copy/" + id);
    },
  });
};
const bophan = (id) => {
  var d = departments.value.find((x) => x.makho == id);
  return d ? d.tenkho : "";
};
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

  Api.kho().then((res) => {
    departments.value = res;
  });
});
const waiting = ref();
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>

<style lang="scss"></style>
