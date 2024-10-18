<template>
  <section class="card card-fluid">
    <div class="card-body" style="overflow: auto; position: relative">
      <div class="card-header d-md-flex align-items-center">
        <PopupDutru></PopupDutru>
      </div>
      <div class="card-body">
        <DataTable
          class="dt-responsive-table"
          showGridlines
          :value="datatable"
          :lazy="true"
          ref="dt"
          scrollHeight="70vh"
          v-model:selection="selectedProducts"
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
            <div class="d-md-flex align-items-center">
              <div style="width: 200px">
                <TreeSelect
                  :options="columns"
                  v-model="showing"
                  multiple
                  :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'"
                >
                </TreeSelect>
              </div>
              <div class="ml-auto">
                <SelectButton
                  v-model="filterTable"
                  :options="list_filterTable"
                  aria-labelledby="basic"
                  :pt="{ button: 'form-control-sm' }"
                  @change="loadLazyData"
                  optionValue="value"
                  :allowEmpty="true"
                >
                  <template #option="slotProps">
                    {{ slotProps.option.label }}
                  </template>
                </SelectButton>
              </div>
              <div class="ml-3">
                <SelectButton
                  v-model="filterTable1"
                  :options="list_filterTable1"
                  aria-labelledby="basic"
                  :pt="{ button: 'form-control-sm' }"
                  @change="loadLazyData"
                  optionValue="value"
                  :allowEmpty="true"
                >
                  <template #option="slotProps">
                    {{ slotProps.option.label }}
                  </template>
                </SelectButton>
              </div>
            </div>
          </template>

          <template #empty> Không có dữ liệu. </template>
          <Column
            v-for="col of selectedColumns"
            :field="col.data"
            :header="col.label"
            :key="col.data"
            :showFilterMatchModes="false"
          >
            <template #body="slotProps">
              <span class="ui-column-title">{{ col.label }}</span>
              <div class="ui-column-data">
                <template v-if="col.data == 'id'">
                  <router-link :to="'/dutru/edit/' + slotProps.data[col.data]">
                    <i class="fas fa-pencil-alt mr-2"></i>
                    {{ slotProps.data[col.data] }}
                  </router-link>
                </template>

                <template v-else-if="col.data == 'name'">
                  <div>
                    <div style="text-wrap: pretty">
                      <router-link
                        :to="'/dutru/edit/' + slotProps.data.id"
                        class="text-blue"
                        >{{ slotProps.data.name }}
                      </router-link>
                    </div>
                    <small
                      >Tạo bởi
                      <i>{{ slotProps.data.user_created_by?.FullName }}</i> lúc
                      {{
                        formatDate(
                          slotProps.data.created_at,
                          "YYYY-MM-DD HH:mm"
                        )
                      }}</small
                    >
                  </div>
                </template>

                <template v-else-if="col.data == 'list_muahang'">
                  <div v-for="item of slotProps.data[col.data]" :key="item.id">
                    <RouterLink
                      :to="'/muahang/edit/' + item.id"
                      class="text-primary mr-2"
                      v-if="is_Cungung"
                      >{{ item.id }} - {{ item.code }}
                    </RouterLink>
                    <RouterLink
                      :to="'/muahang/nhanhang/' + item.id"
                      class="text-primary mr-2"
                      v-else-if="
                        item['is_dathang'] &&
                        ((item['loaithanhtoan'] == 'tra_sau' &&
                          !item['is_nhanhang']) ||
                          (item['loaithanhtoan'] == 'tra_truoc' &&
                            item['is_thanhtoan']))
                      "
                    >
                      {{ item.id }} - {{ item.code }}
                    </RouterLink>
                    <span class="mr-2" v-else
                      >{{ item.id }} - {{ item.code }}
                    </span>
                    <Tag
                      value="Hoàn thành"
                      severity="success"
                      v-if="item['date_finish']"
                    />
                    <Tag
                      value="Chờ nhận hàng"
                      severity="info"
                      v-else-if="
                        item['is_dathang'] &&
                        ((item['loaithanhtoan'] == 'tra_sau' &&
                          !item['is_nhanhang']) ||
                          (item['loaithanhtoan'] == 'tra_truoc' &&
                            item['is_thanhtoan']))
                      "
                    />
                    <Tag
                      value="Chờ thanh toán"
                      severity="info"
                      v-else-if="
                        item['is_dathang'] &&
                        ((item['loaithanhtoan'] == 'tra_truoc' &&
                          !item['is_thanhtoan']) ||
                          (item['loaithanhtoan'] == 'tra_sau' &&
                            item['is_nhanhang']))
                      "
                    />
                    <Tag
                      value="Đang thực hiện"
                      severity="secondary"
                      v-else-if="
                        item['status_id'] == 1 ||
                        item['status_id'] == 6 ||
                        item['status_id'] == 7
                      "
                    />

                    <Tag
                      value="Đang trình ký"
                      severity="warning"
                      v-else-if="item['status_id'] == 9"
                    />
                    <Tag
                      value="Đang đặt hàng"
                      v-else-if="item['status_id'] == 10"
                    />
                    <Tag
                      value="Không duyệt"
                      severity="danger"
                      v-else-if="item['status_id'] == 11"
                    />
                  </div>
                </template>
                <template v-else-if="col.data == 'priority_id'">
                  <div class="text-center">
                    <Tag
                      value="Bình thường"
                      size="small"
                      v-if="slotProps.data['priority_id'] == 1"
                    />
                    <Tag
                      value="Ưu tiên"
                      severity="warning"
                      size="small"
                      v-else-if="slotProps.data['priority_id'] == 2"
                    />
                    <Tag
                      value="Gấp"
                      severity="danger"
                      size="small"
                      v-else-if="slotProps.data['priority_id'] == 3"
                    />
                  </div>
                </template>

                <template v-else-if="col.data == 'bophan_id'">
                  {{ bophan(slotProps.data.bophan_id) }}
                </template>
                <template v-else-if="col.data == 'status_id'">
                  <div class="text-center">
                    <Tag
                      value="Hoàn thành"
                      severity="success"
                      size="small"
                      v-if="slotProps.data['date_finish']"
                    />
                    <Tag
                      value="Nháp"
                      severity="secondary"
                      size="small"
                      v-else-if="slotProps.data[col.data] == 1"
                    />
                    <Tag
                      value="Đang trình ký"
                      severity="warning"
                      size="small"
                      v-else-if="slotProps.data[col.data] == 2"
                    />
                    <Tag
                      value="Chờ ký duyệt"
                      severity="warning"
                      size="small"
                      v-else-if="slotProps.data[col.data] == 3"
                    />
                    <Tag
                      value="Đã duyệt"
                      severity="success"
                      size="small"
                      v-else-if="slotProps.data[col.data] == 4"
                    />
                    <Tag
                      value="Không duyệt"
                      severity="danger"
                      size="small"
                      v-else-if="slotProps.data[col.data] == 5"
                    />
                  </div>
                </template>

                <div v-else v-html="slotProps.data[col.data]"></div>
              </div>
            </template>

            <template
              #filter="{ filterModel, filterCallback }"
              v-if="col.filter == true"
            >
              <template v-if="col.data == 'priority_id'">
                <select
                  class="form-control"
                  v-model="filterModel.value"
                  @change="filterCallback()"
                >
                  <option value="1">Bình thường</option>
                  <option value="2">Ưu tiên</option>
                  <option value="3">Gấp</option>
                </select>
              </template>
              <div v-else-if="col.data == 'bophan_id'" style="width: 200px">
                <DepartmentOfUserTreeSelect
                  v-model="filterModel.value"
                  @update:model-value="filterCallback()"
                >
                </DepartmentOfUserTreeSelect>
              </div>
              <template v-else>
                <InputText
                  type="text"
                  v-model="filterModel.value"
                  @keydown.enter="filterCallback()"
                  class="p-column-filter"
              /></template>
            </template>
          </Column>
          <Column style="width: 1rem">
            <template #body="slotProps">
              <a
                class="p-link text-danger font-16"
                @click="confirmDelete(slotProps.data['id'])"
                v-if="slotProps.data.created_by == user.id"
                ><i class="pi pi-trash"></i
              ></a>
            </template>
          </Column>
        </DataTable>
      </div>
    </div>
  </section>
  <Loading :waiting="waiting"></Loading>
</template>
<script setup>
import { onMounted, ref, computed, watch } from "vue";
import dutruApi from "../../api/dutruApi";
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
import PopupDutru from "./PopupDutru.vue";
import DepartmentOfUserTreeSelect from "../TreeSelect/DepartmentOfUserTreeSelect.vue";
import Api from "../../api/Api";
const store = useAuth();
const {
  is_admin,
  is_Cungung,
  is_Ketoan,
  is_CungungNVL,
  is_Qa,
  is_CungungGiantiep,
  is_CungungHCTT,
  user,
} = storeToRefs(store);
// const props = defineProps({
//   type: Number
// })
const confirm = useConfirm();
const datatable = ref();
const department_id = ref();
const list_filterTable = ref([{ label: "Tôi tạo", value: 1 }]);
const list_filterTable1 = ref([]);
const filterTable = ref(1);
const filterTable1 = ref();
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
    label: "Bộ phận dự trù",
    data: "bophan_id",
    className: "text-center",
    filter: true,
  },
  {
    id: 4,
    label: "Độ ưu tiên",
    data: "priority_id",
    className: "text-center",
    filter: true,
  },
  {
    id: 5,
    label: "Trạng thái",
    data: "status_id",
    className: "text-center",
  },
  {
    id: 6,
    label: "DNMH",
    data: "list_muahang",
    className: "text-center",
  },
]);
const filters = ref({
  id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  code: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { value: null, matchMode: FilterMatchMode.CONTAINS },
  bophan_id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  priority_id: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const departments = ref([]);
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
    type_id: filterTable1.value,
    type1: filterTable.value,
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
const bophan = (id) => {
  var d = departments.value.find((x) => x.id == id);
  return d ? d.name : "";
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
  fill();
  loadLazyData();

  Api.departmentsofuser().then((res) => {
    departments.value = res;
  });
});
const fill = () => {
  if (is_Cungung.value) {
    list_filterTable.value.push({ label: "Phân công cho tôi", value: 2 });
    filterTable.value = 2;
  }

  list_filterTable1.value.push({ label: "Mua hàng gián tiếp", value: 2 });

  list_filterTable1.value.push({ label: "Hóa chất,thuốc thử QC", value: 3 });

  list_filterTable1.value.push({ label: "Nguyên vật liệu", value: 1 });
};
const waiting = ref();
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>

<style lang="scss"></style>
