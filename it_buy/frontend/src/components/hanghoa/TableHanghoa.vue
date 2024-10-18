<template>
  <DataTable
    size="small"
    class="p-datatable-customers"
    showGridlines
    :value="datatable1"
    :lazy="true"
    ref="dt"
    scrollHeight="70vh"
    v-model:selection="selected"
    :paginator="true"
    :rowsPerPageOptions="[10, 20, 50, 100]"
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
    contextMenu
    v-model:contextMenuSelection="selectedItem"
    @rowContextmenu="onRowContextMenu"
  >
    <template #header>
      <div class="d-flex align-items-center">
        <Button
          label="Tạo đề nghị mua hàng"
          icon="pi pi-plus"
          class="p-button-success p-button-sm"
          :disabled="!selected || !selected.length"
          v-if="is_CungungGiantiep || is_CungungNVL || is_CungungHCTT"
          @click="taodenghimuahang()"
        ></Button>
        <Button
          label="Xuất excel"
          icon="pi pi-download"
          class="p-button-sm ml-2"
          @click="xuatexcel"
        ></Button>
        <div class="ml-auto">
          <SelectButton
            v-model="filterTable1"
            :options="['Đã xử lý', 'Chưa xử lý']"
            aria-labelledby="basic"
            :pt="{ button: 'form-control-sm' }"
            @change="loadLazyData"
          />
        </div>
        <div class="ml-3">
          <SelectButton
            v-model="filterTable"
            :options="list_filterTable"
            aria-labelledby="basic"
            :pt="{ button: 'form-control-sm' }"
            @change="changeFilterTable"
            optionValue="value"
            :allowEmpty="true"
          >
            <template #option="slotProps">
              {{ slotProps.option.label }}
            </template>
          </SelectButton>
        </div>
      </div>

      <div class="d-inline-flex float-right">
        <!-- <Button label="Chi tiết" class="p-button-primary p-button-sm" @click="chitiet()"
            v-if="type == 'tonghop'"></Button>
          <Button label="Tổng hợp" class="p-button-warning p-button-sm" @click="tonghop()"
            v-else-if="type == 'chitiet'"></Button> -->
      </div>
    </template>
    <template #footer>
      <div style="text-align: center">
        <div class="row mb-2">
          <div class="col-md">Các loại trạng thái DNMH</div>
        </div>
        <div class="row justify-content-center" style="gap: 20px">
          <div class="col-md">
            <Tag value="Đang thực hiện" severity="secondary" />
          </div>
          <div class="col-md">
            <Tag value="Đang trình ký" severity="warning" />
          </div>
          <div class="col-md">
            <Tag value="Không duyệt" severity="danger" />
          </div>
          <div class="col-md">
            <Tag value="Đang đặt hàng" />
          </div>
          <div class="col-md">
            <Tag value="Chờ nhận hàng" severity="info" />
          </div>
          <div class="col-md">
            <Tag value="Chờ thanh toán" severity="info" />
          </div>
          <div class="col-md">
            <Tag value="Hoàn thành" severity="success" />
          </div>
        </div>
      </div>
    </template>
    <template #empty>
      <div class="text-center">Không có dữ liệu.</div>
    </template>
    <Column selectionMode="multiple"></Column>
    <Column
      v-for="col in selectedColumns"
      :field="col.data"
      :header="col.label"
      :key="col.data"
      :showFilterMatchModes="false"
    >
      <template #body="slotProps">
        <template v-if="col.data == 'list_dutru'">
          <RouterLink
            :to="'/dutru/edit/' + slotProps.data.dutru.id"
            class="text-primary"
            target="_blank"
            >{{ slotProps.data.dutru.code }}</RouterLink
          >
          <Tag
            value="Bình thường"
            size="small"
            class="ml-2"
            v-if="slotProps.data.dutru['priority_id'] == 1"
          />
          <Tag
            value="Ưu tiên"
            severity="warning"
            size="small"
            class="ml-2"
            v-else-if="slotProps.data.dutru['priority_id'] == 2"
          />
          <Tag
            value="Gấp"
            severity="danger"
            size="small"
            class="ml-2"
            v-else-if="slotProps.data.dutru['priority_id'] == 3"
          />
        </template>

        <template v-else-if="col.data == 'list_muahang'">
          <div v-for="item of slotProps.data[col.data]" :key="item.id">
            <RouterLink
              :to="'/muahang/edit/' + item.id"
              class="text-primary mr-2"
              target="_blank"
              v-if="is_CungungGiantiep || is_CungungNVL || is_CungungHCTT"
              >{{ item.id }} - {{ item.code }}
            </RouterLink>
            <span class="mr-2" v-else>{{ item.id }} - {{ item.code }}</span>
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
                ((item['loaithanhtoan'] == 'tra_sau' && !item['is_nhanhang']) ||
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
                  (item['loaithanhtoan'] == 'tra_sau' && item['is_nhanhang']))
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
            <Tag value="Đang đặt hàng" v-else-if="item['status_id'] == 10" />
            <Tag
              value="Không duyệt"
              severity="danger"
              v-else-if="item['status_id'] == 11"
            />
          </div>
        </template>

        <template v-else-if="col.data == 'ngayhethan'">
          {{ formatDate(slotProps.data.dutru.date) }}
        </template>

        <template v-else-if="col.data == 'thanhtien'">
          <div v-if="is_CungungGiantiep || is_CungungNVL || is_CungungHCTT">
            {{ formatPrice(slotProps.data[col.data], 0) }} VNĐ
          </div>
        </template>

        <template v-else-if="col.data == 'soluong_dutru'">
          {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
        </template>

        <template v-else-if="col.data == 'bophan'">
          {{ bophan(slotProps.data.dutru.bophan_id) }}
        </template>
        <template v-else-if="col.data == 'soluong_mua'">
          {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
        </template>

        <template v-else-if="col.data == 'soluong'">
          {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
        </template>
        <template v-else-if="col.data == 'mahh'">
          {{ slotProps.data[col.data] }}
          <i
            class="fas fa-sync-alt"
            style="cursor: pointer"
            @click="openNew(slotProps.data)"
          ></i>
        </template>

        <template v-else-if="col.data == 'tenhh'">
          <div class="d-flex">
            <div>
              <div
                style="
                  word-break: break-all;
                  white-space: pre-line;
                  display: flex;
                "
                :class="{ 'text-danger': slotProps.data.is_new == true }"
              >
                {{ slotProps.data[col.data] }}
              </div>
              <div class="small">
                <span v-if="slotProps.data.user">
                  Phân công cho <i>{{ slotProps.data.user.FullName }}</i></span
                >
                <Tag
                  severity="secondary"
                  :value="tag"
                  v-for="tag in slotProps.data.list_tag"
                  class="ml-2"
                  :key="tag"
                ></Tag>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          {{ slotProps.data[col.data] }}
        </template>
      </template>

      <template
        #filter="{ filterModel, filterCallback }"
        v-if="col.filter == true"
      >
        <div v-if="col.data == 'tenhh'">
          <div class="row" style="width: 400px">
            <div class="col-12">
              <b>Tên:</b>
              <InputText
                type="text"
                v-model="filterModel.value"
                @keydown.enter="filterCallback()"
                class="p-column-filter"
              />
            </div>
            <div class="col-12">
              <b>Phân cho:</b>
              <UserTreeSelect v-model="customFilter.user_id"></UserTreeSelect>
            </div>
            <div class="col-12">
              <b>Tags:</b>
              <InputText
                type="text"
                v-model="customFilter.tags"
                class="p-column-filter"
              />
            </div>
          </div>
        </div>
        <div v-else-if="col.data == 'bophan'" style="width: 200px">
          <DepartmentOfUserTreeSelect
            v-model="filterModel.value"
            @update:model-value="filterCallback()"
          >
          </DepartmentOfUserTreeSelect>
        </div>
        <div v-else>
          <InputText
            type="text"
            v-model="filterModel.value"
            @keydown.enter="filterCallback()"
            class="p-column-filter"
          />
        </div>
      </template>
      <template #filterclear="{ filterCallback }" v-if="col.filter == true">
        <Button
          label="Clear"
          size="small"
          outlined
          @click="customClearFilter(col.data, filterCallback)"
        ></Button>
      </template>
    </Column>
  </DataTable>
  <ContextMenu ref="cm" :model="menuModel" />
  <Dialog
    v-model:visible="visiblePhancong"
    header="Phân công"
    :modal="true"
    style="width: 50vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <UserTreeSelect v-model="selectedItem.user_id"></UserTreeSelect>
    <div class="d-flex justify-content-center mt-2">
      <Button
        type="button"
        label="Lưu lại"
        severity="success"
        @click="phancong"
        size="small"
      ></Button>
    </div>
  </Dialog>

  <Dialog
    v-model:visible="visibleTag"
    header="Tag"
    :modal="true"
    style="width: 50vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <div class="row">
      <div class="col-12">
        <Chips v-model="selectedItem.list_tag" class="d-inline-block w-100" />
      </div>
    </div>
    <div class="d-flex justify-content-center mt-2">
      <Button
        type="button"
        label="Lưu lại"
        severity="success"
        @click="tag"
        size="small"
      ></Button>
    </div>
  </Dialog>
  <Dialog
    v-model:visible="visibleDialog"
    header="Đổi mã hàng hóa"
    :modal="true"
    style="width: 75vw"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }"
  >
    <div class="row mb-2">
      <div class="field col">
        <label for="name">Chuyển từ</label>
        <div>
          {{ modelDialog.from_hh }}
        </div>
      </div>
      <div class="field col">
        <label for="name">sang</label>
        <div>
          <MaterialTreeSelect
            v-model="modelDialog.to_id"
            :useID="false"
            @update:model-value="changeMahh()"
          >
          </MaterialTreeSelect>
        </div>
      </div>
      <div class="field col">
        <label for="name">Thông báo đến người dự trù</label>
        <div>
          <Button
            label="Thông báo"
            icon="far fa-paper-plane"
            size="small"
            class="mr-2"
            @click="thongbaodoima"
          ></Button>
          <Button
            label="Lưu lại"
            icon="pi pi-check"
            size="small"
            severity="success"
            @click="savedoima"
          ></Button>
        </div>
      </div>
    </div>
    <div class="row">
      <div class="col-12">Lịch sử mua hàng của mã {{ modelDialog.to }}</div>
      <div class="col-12 mt-2">
        <table class="table table-border">
          <thead>
            <tr>
              <th>Đề nghị mua hàng</th>
              <th>Hàng hóa</th>
              <th>Số lượng</th>
              <th>Đơn giá</th>
              <th>Thành tiền</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in modelDialog.history" :key="item.id">
              <th>
                <RouterLink :to="'/muahang/edit/' + item.muahang.id"
                  >{{ item.muahang.code }} - {{ item.muahang.name }}
                </RouterLink>
              </th>
              <th>{{ item.tenhh }}</th>
              <th>{{ item.soluong }}</th>
              <th>{{ formatPrice(item.dongia, 2) }} {{ item.tiente }}</th>
              <th>{{ formatPrice(item.thanhtien, 2) }} {{ item.tiente }}</th>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </Dialog>

  <PopupAdd @save="saveDanhgia"></PopupAdd>
  <Loading :waiting="waiting"></Loading>
</template>
<script setup>
import { onMounted, ref, computed, watch } from "vue";
import dutruApi from "../../api/dutruApi";
import Chips from "primevue/chips";
import Tag from "primevue/tag";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import ContextMenu from "primevue/contextmenu";
import Column from "primevue/column"; ////Datatable
import InputText from "primevue/inputtext";
import SelectButton from "primevue/selectbutton";
import { formatDate, formatPrice } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import { useRoute, useRouter } from "vue-router";
import { rand } from "../../utilities/rand";
import MaterialTreeSelect from "../../components/TreeSelect/MaterialTreeSelect.vue";

import muahangApi from "../../api/muahangApi";
import { change } from "@syncfusion/ej2-grids";
import { useToast } from "primevue/usetoast";
import Loading from "../Loading.vue";
import UserTreeSelect from "../../components/TreeSelect/UserTreeSelect.vue";
import { useAuth } from "../../stores/auth";
import DepartmentOfUserTreeSelect from "../TreeSelect/DepartmentOfUserTreeSelect.vue";
import PopupAdd from "../danhgianhacungcap/PopupAdd.vue";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
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

const list_filterTable = ref([]);
const filterTable1 = ref();

const store_danhgianhacungcap = useDanhgianhacungcap();
const danhgianhacungcap = storeToRefs(store_danhgianhacungcap);
const taodanhgia = () => {
  store_danhgianhacungcap.openNew();
  danhgianhacungcap.model.value.list_dutru_chitiet = selected.value.map(
    (item) => {
      return item.id;
    }
  );
  let select = selected.value[0];
  danhgianhacungcap.model.value.tenhh = select.tenhh;
  danhgianhacungcap.model.value.dvt = select.dvt;
  danhgianhacungcap.model.value.grade = select.grade;
  danhgianhacungcap.model.value.masothietke = select.masothietke;
  danhgianhacungcap.model.value.nhasx = select.nhasx;
};
const saveDanhgia = (id) => {
  // console.log();
  router.push("/danhgianhacungcap/details/" + id);
};
const changeFilterTable = () => {
  selected.value = null;
  loadLazyData();
};
const checkdanhgia = computed(() => {
  if (selected.value && selected.value.length > 0) {
    if (selected.value[0].dutru.type_id == 1) return false;
  }
  return true;
});
const customClearFilter = (col, callback) => {
  if (col == "tenhh") customFilter.value.user_id = null;
  callback();
};
const xuatexcel = () => {
  loading.value = true;
  dutruApi.xuatexcel(lazyParams.value).then((res) => {
    loading.value = false;
    if (res.success) {
      window.open(res.link, "_blank");
    } else {
      alert(res.message);
    }
  });
};
const filterTable = ref();
const props = defineProps({
  type: Number,
  dutru_id: [Number, String],
});
const toast = useToast();
const store_muahang = useMuahang();
const { datatable } = storeToRefs(store_muahang);
const router = useRouter();
const datatable1 = ref();
const selected = ref();
const columns = ref([
  {
    id: 0,
    label: "Mã",
    data: "mahh",
    className: "text-center",
    filter: true,
  },

  {
    id: 1,
    label: "Tên",
    data: "tenhh",
    className: "text-center",
    filter: true,
  },
  {
    id: 2,
    label: "Tên sản phẩm",
    data: "tensp",
    className: "text-center",
    filter: true,
  },
  {
    id: 3,
    label: "Bộ phận dự trù",
    data: "bophan",
    className: "text-center",
    filter: true,
  },
  {
    id: 4,
    label: "Số lượng dự trù",
    data: "soluong_dutru",
    className: "text-center",
  },

  {
    id: 5,
    label: "Mã dự trù",
    data: "list_dutru",
    className: "text-center",
    filter: true,
  },
  {
    id: 6,
    label: "Hạn giao hàng",
    data: "ngayhethan",
    className: "text-center",
  },

  {
    id: 7,
    label: "DNMH",
    data: "list_muahang",
    className: "text-center",
  },
  {
    id: 8,
    label: "Số lượng mua",
    data: "soluong_mua",
    className: "text-center",
  },
  {
    id: 9,
    label: "Còn lại",
    data: "soluong",
    className: "text-center",
  },
  {
    id: 10,
    label: "Tổng tiền",
    data: "thanhtien",
    className: "text-center",
  },
]);
const filters = ref({
  id: { value: null, matchMode: FilterMatchMode.CONTAINS },
  mahh: { value: null, matchMode: FilterMatchMode.CONTAINS },
  tenhh: { value: null, matchMode: FilterMatchMode.CONTAINS },
  tensp: { value: null, matchMode: FilterMatchMode.CONTAINS },
  bophan: { value: null, matchMode: FilterMatchMode.CONTAINS },
  list_dutru: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const customFilter = ref({
  user_id: null,
  tags: null,
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_dutru_chitiet"; ////
const first = ref(0);
const rows = ref(20);
const draw = ref(0);
const department_id = ref();
const selectedColumns = computed(() => {
  return columns.value.filter((col) => showing.value.includes(col.id));
});
const lazyParams = computed(() => {
  let data_filters = {};
  for (let key in filters.value) {
    data_filters[key] = filters.value[key].value;
  }
  for (let key in customFilter.value) {
    data_filters[key] = customFilter.value[key];
  }
  return {
    draw: draw.value,
    start: first.value,
    length: rows.value,
    filters: data_filters,
    type_id: filterTable.value,
    dutru_id: props.dutru_id,
    filterTable: filterTable1.value,
  };
});
const dt = ref(null);

////Data table
const loadLazyData = () => {
  loading.value = true;
  dutruApi.tableChitiet(lazyParams.value).then((res) => {
    // console.log(res);
    datatable1.value = res.data;
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
const taodenghimuahang = () => {
  // console.log(selected);
  store_muahang.reset();
  if (selected.value.length) {
    for (var item of selected.value) {
      if (!(item.soluong > 0)) {
        alert("Hàng hóa đã mua đủ số lượng!");
        return false;
      }
    }
  }

  router.push("/muahang/add?no_reset=1");
  var clonedSelected = JSON.parse(JSON.stringify(selected.value));
  datatable.value = clonedSelected.map((item, key) => {
    item.type_id = item.dutru.type_id;
    item.dvt_dutru = item.dvt;
    item.quidoi = 1;
    item.soluong_dutru = item.soluong;
    item.stt = key + 1;
    item.ids = rand();
    delete item.dutru;
    delete item.list_muahang;
    delete item.id;
    return item;
  });
};
const departments = ref([]);
const visiblePhancong = ref();
const visibleTag = ref();
const cm = ref();
const selectedItem = ref();
const menuModel = ref([
  {
    label: "Phân công",
    icon: "pi pi-fw pi-user",
    command: () => openPhancong(),
  },
  { label: "Tags", icon: "pi pi-fw pi-tag", command: () => openTag() },
]);
const onRowContextMenu = (event) => {
  cm.value.show(event.originalEvent);
};
const openPhancong = () => {
  if (!selected.value) selected.value = [];
  selected.value.push(selectedItem.value);
  visiblePhancong.value = true;
};
const openTag = () => {
  visibleTag.value = true;
};
const phancong = () => {
  if (!selectedItem.value.user_id) {
    alert("Chưa chọn User!");
    return false;
  }
  var All = [];
  let unique = [...new Set(selected.value)];
  for (var item of unique) {
    All.push(
      dutruApi.phancong({ id: item.id, user_id: selectedItem.value.user_id })
    );
  }
  Promise.all(All).then((response) => {
    visiblePhancong.value = false;
    selected.value = null;
    selectedItem.value = {};
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Phân công cho người dùng",
      life: 3000,
    });
    loadLazyData();
  });
};

const tag = () => {
  //console.log(selectedItem.value);
  if (!selectedItem.value.list_tag) {
    alert("Chưa nhập tag");
    return false;
  }
  dutruApi
    .tag({ id: selectedItem.value.id, list_tag: selectedItem.value.list_tag })
    .then((response) => {
      visibleTag.value = false;

      selectedItem.value = {};
      toast.add({
        severity: "success",
        summary: "Thành công",
        life: 3000,
      });
      loadLazyData();
    });
};

const visibleDialog = ref();
const modelDialog = ref({});
const openNew = async (row) => {
  visibleDialog.value = true;
  modelDialog.value.id = row.id;
  modelDialog.value.from_hh = row.mahh;
  modelDialog.value.to_id = row.mahh;
  changeMahh();
};
const savedoima = async () => {
  visibleDialog.value = false;
  var res = await dutruApi.savedoima({
    dutru_chitiet_id: modelDialog.value.id,
    mahh: modelDialog.value.to_id,
  });
  if (res.success) {
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Thành công",
      life: 3000,
    });
    loadLazyData();
  }
};
const thongbaodoima = async () => {
  // visibleDialog.value = false;
  var res = await dutruApi.thongbaodoima({
    dutru_chitiet_id: modelDialog.value.id,
    mahh: modelDialog.value.to_id,
  });
  if (res.success) {
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Thành công",
      life: 3000,
    });
  }
};
const changeMahh = async () => {
  var res = await muahangApi.getHistory(modelDialog.value.to_id);
  // console.log(res);
  modelDialog.value.to = res.to;
  modelDialog.value.history = res.data;
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
  if (props.dutru_id > 0) return;
  list_filterTable.value.push({ label: "Mua hàng gián tiếp", value: 2 });

  list_filterTable.value.push({ label: "Hóa chất,thuốc thử QC", value: 3 });

  list_filterTable.value.push({ label: "Nguyên vật liệu", value: 1 });

  if (list_filterTable.value.length > 0) {
    filterTable.value = list_filterTable.value[0].value;
  }
};
const waiting = ref();
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>
<style lang="scss"></style>
