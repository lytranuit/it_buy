﻿<template>
  <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt" scrollHeight="70vh"
    :paginator="true" :rowsPerPageOptions="[10, 50, 100]" :rows="rows" :totalRecords="totalRecords"
    @page="onPage($event)" :rowHover="true" :loading="loading" responsiveLayout="scroll" :resizableColumns="true"
    columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu">
    <template #header>
      <div class="d-flex align-items-center">
        <div style="width: 200px">
          <TreeSelect :options="columns" v-model="showing" multiple :limit="0"
            :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
          </TreeSelect>
        </div>
        <div class="ml-auto">
          <SelectButton v-model="filterTable" :options="list_filterTable" aria-labelledby="basic"
            :pt="{ 'button': 'form-control-sm' }" @change="loadLazyData" optionValue="value" :allowEmpty="false">
            <template #option="slotProps">
              {{ slotProps.option.label }}
            </template>
          </SelectButton>
        </div>

      </div>
    </template>

    <template #empty> Không có dữ liệu. </template>

    <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
      :showFilterMatchModes="false">

      <template #body="slotProps">
        <template v-if="col.data == 'id'">
          <RouterLink :to="'/muahang/edit/' + slotProps.data[col.data]">
            <i class="fas fa-pencil-alt mr-2"></i>
            {{ slotProps.data[col.data] }}
          </RouterLink>
        </template>

        <template v-else-if="col.data == 'name'">
          <div>
            <div style="text-wrap: pretty;">
              <RouterLink :to="'/muahang/edit/' + slotProps.data.id" class="text-blue">{{ slotProps.data.name }}
              </RouterLink>
            </div>
            <small>Tạo bởi <i>{{ slotProps.data.user_created_by?.FullName }}</i> lúc {{
    formatDate(slotProps.data.created_at, "YYYY-MM-DD HH:mm") }}</small>
          </div>
        </template>


        <template v-else-if="col.data == 'tonggiatri'">
          {{ formatPrice(slotProps.data[col.data], 0) }} {{ slotProps.data["tiente"] }}
        </template>
        <template v-else-if="col.data == 'status_id'">
          <div class="text-center">
            <Tag value="Hoàn thành" severity="success" v-if="slotProps.data['date_finish']" />
            <Tag value="Chờ nhận hàng" severity="info"
              v-else-if="slotProps.data['is_dathang'] && ((slotProps.data['loaithanhtoan'] == 'tra_sau' && !slotProps.data['is_nhanhang']) || (slotProps.data['loaithanhtoan'] == 'tra_truoc' && slotProps.data['is_thanhtoan']))" />
            <Tag value="Chờ thanh toán" severity="info"
              v-else-if="slotProps.data['is_dathang'] && ((slotProps.data['loaithanhtoan'] == 'tra_truoc' && !slotProps.data['is_thanhtoan']) || (slotProps.data['loaithanhtoan'] == 'tra_sau' && slotProps.data['is_nhanhang']))" />
            <Tag value="Đang thực hiện" severity="secondary" v-else-if="slotProps.data[col.data] == 1" />
            <Tag value="Đang gửi và nhận báo giá" severity="warning" v-else-if="slotProps.data[col.data] == 6" />
            <Tag value="So sánh giá" severity="warning" v-else-if="slotProps.data[col.data] == 7" />
            <Tag value="Đang trình ký" severity="warning" v-else-if="slotProps.data[col.data] == 8" />
            <Tag value="Chờ ký duyệt" severity="warning" v-else-if="slotProps.data[col.data] == 9" />
            <Tag value="Đã duyệt" v-else-if="slotProps.data[col.data] == 10" />
            <Tag value="Không duyệt" severity="danger" v-else-if="slotProps.data[col.data] == 11" />
          </div>
          <div class="text-center mt-2">

          </div>
        </template>
        <div v-else v-html="slotProps.data[col.data]"></div>
      </template>
      <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
        <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()" class="p-column-filter" />
      </template>
    </Column>
    <Column style="width: 1rem">
      <template #body="slotProps">
        <a class="p-link text-danger font-16" @click="confirmDelete(slotProps.data['id'])"
          v-if="slotProps.data.created_by == user.id"><i class="pi pi-trash"></i></a>
      </template>
    </Column>
  </DataTable>
  <Loading :waiting="waiting"></Loading>
</template>
<script setup>
import { onMounted, ref, computed, watch } from "vue";
import muahangApi from "../../api/muahangApi";
import Tag from "primevue/tag";
import Button from "primevue/button";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column"; ////Datatable
import InputText from "primevue/inputtext";
import ConfirmDialog from "primevue/confirmdialog";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../components/Loading.vue";
import { formatDate, formatPrice } from "../../utilities/util";
import { useAuth } from "../../stores/auth";
import { storeToRefs } from "pinia";
import SelectButton from "primevue/selectbutton";
const store = useAuth();
const { is_admin, is_Cungung, is_Ketoan, is_CungungNVL, is_Qa, is_CungungGiantiep, is_CungungHCTT, user } = storeToRefs(store);
const props = defineProps({
  filter_thanhtoan: Boolean
})
const list_filterTable = ref([]);
const filterTable = ref();
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
  {
    id: 5,
    label: "Tổng giá trị",
    data: "tonggiatri",
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
const column_cache = "columns_muahang"; ////
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
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
    type_id: filterTable.value,
    filter_thanhtoan: props.filter_thanhtoan,
    filters: data_filters
  };
});
const dt = ref(null);

////Data table
const loadLazyData = () => {
  loading.value = true;
  muahangApi.table(lazyParams.value).then((res) => {
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
      muahangApi.delete(id).then((res) => {
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
  fill();
  loadLazyData();
});
const fill = () => {
  if (props.filter_thanhtoan > 0)
    return;
  if (is_CungungGiantiep.value) {
    list_filterTable.value.push({ label: "Mua hàng gián tiếp", value: 2 });

    // { label: "Nguyên vật liệu", value: 1 },
    // { label: "Hóa chất,thuốc thử QC", value: 3 },
  }

  if (is_CungungHCTT.value) {
    list_filterTable.value.push({ label: "Hóa chất,thuốc thử QC", value: 3 });
  }
  if (is_CungungNVL.value) {
    list_filterTable.value.push({ label: "Nguyên vật liệu", value: 1 });
  }

  if (list_filterTable.value.length > 0) {
    filterTable.value = list_filterTable.value[0].value;
  }
}
const waiting = ref();
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});
</script>
<style lang="scss"></style>
