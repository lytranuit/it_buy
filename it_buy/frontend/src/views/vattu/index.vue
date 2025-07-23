<template>
  <div class="row clearfix">
    <div class="col-12">
      <h5 class="card-header drag-handle">
        <RouterLink to="vattu/phieunhap/add">
          <Button label="Nhập" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"></Button>
        </RouterLink>
        <Button label="Xuất" icon="pi pi-minus" class="p-button-primary p-button-sm mr-2" @click="taophieuxuat"
          :disabled="!selected || !selected.length"></Button>
        <Button label="Xuất Excel" icon="pi pi-file-excel" class="p-button-warning p-button-sm mr-2"
          @click="excel"></Button>
      </h5>
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <Datatable class="p-datatable-customers" showGridlines :value="datatable1" :lazy="true" ref="dt"
            scrollHeight="70vh" v-model:selection="selected" :paginator="true" :rowsPerPageOptions="[10, 50, 100]"
            :rows="rows" :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters"
            filterDisplay="menu">
            <template #header>
              <div style="width: 200px">
                <TreeSelect :options="columns" v-model="showing" multiple :limit="0"
                  :limitText="(count) => 'Hiển thị: ' + count + ' cột'" :normalizer="normalizer">
                </TreeSelect>
              </div>
            </template>

            <template #empty> Không có dữ liệu. </template>
            <Column selectionMode="multiple" style="width: 3rem" :exportable="false"></Column>
            <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
              :showFilterMatchModes="false">
              <template #body="slotProps">
                <template v-if="col.data == 'id'">
                  <RouterLink :to="'/user/edit/' + slotProps.data[col.data]">
                    <i class="fas fa-pencil-alt mr-2"></i>
                    {{ slotProps.data[col.data] }}
                  </RouterLink>
                </template>
                <template v-else-if="col.data == 'handung'">
                  {{ formatDate(slotProps.data[col.data]) }}
                </template>
                <template v-else-if="col.data == 'soluong'">
                  {{ formatPrice(slotProps.data[col.data]) }} {{ slotProps.data.dvt }}
                </template>
                <div v-else v-html="slotProps.data[col.data]"></div>
              </template>
              <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                  class="p-column-filter" />
              </template>
            </Column>
          </Datatable>
        </div>
      </section>
    </div>

    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import vattuApi from "../../api/vattuApi";
import Button from "primevue/button";
import Datatable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column"; ////datatable1
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import Loading from "../../components/Loading.vue";
import { formatPrice, formatDate } from "../../utilities/util";
import { useVattu } from "../../stores/Vattu";
import { storeToRefs } from "pinia";
import { rand } from "../../utilities/rand";
import { useRouter } from "vue-router";
const router = useRouter();
const store_vattu = useVattu();
const { datatable, model } = storeToRefs(store_vattu);
const confirm = useConfirm();
const columns = ref([
  {
    label: "Mã hàng hóa",
    data: "mahh",
    className: "text-center",
    filter: true,
  },

  {
    label: "Tên hàng hóa",
    data: "tenhh",
    className: "text-center",
    filter: true,
  },
  {
    label: "Số lô",
    data: "malo",
    className: "text-center",
  },
  {
    label: "Hạn dùng",
    data: "handung",
    className: "text-center",
  },
  {
    label: "Mã nhà cung cấp",
    data: "mancc",
    className: "text-center",
    filter: true,
  },
  {
    label: "Tên nhà cung cấp",
    data: "tenncc",
    className: "text-center",
    filter: true,
  },
  {
    label: "Mã nhà sản xuất",
    data: "mansx",
    className: "text-center",
    filter: true,
  },
  {
    label: "Tên nhà sản xuất",
    data: "tennsx",
    className: "text-center",
    filter: true,
  },
  {
    label: "Số lượng",
    data: "soluong",
    className: "text-center",
  },
  {
    label: "Mã kho",
    data: "makho",
    className: "text-center",
    filter: true,
  }
]);
const filters = ref({
});
const normalizer = (node) => {
  return {
    id: node.data,
    label: node.label,
  };
};
const datatable1 = ref([]);
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_vattu"; ////
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
const selected = ref();
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
  };
});
const dt = ref(null);

const taophieuxuat = () => {
  // console.log(selected);
  store_vattu.reset();
  var makho = selected.value[0].makho;
  if (selected.value.length) {
    for (var item of selected.value) {
      if (makho != item.makho) {
        alert("Hàng hóa không cùng kho!");
        return false;
      }
    }
  }

  router.push("/vattu/phieuxuat/add?no_reset=1");
  var clonedSelected = JSON.parse(JSON.stringify(selected.value));
  datatable.value = clonedSelected.map((item, key) => {
    item.tenhh = item.tenhh;
    item.dvt = item.dvt;
    item.soluong = item.soluong;
    item.tonkho = item.soluong;
    item.mahh = item.mahh;
    item.mancc = item.mancc;
    item.kt_xuat = true;
    // item.soluong_quidoi = item.soluong;
    item.stt = key + 1;
    item.ids = rand();
    return item;
  });

  // console.log(datatable.value);
  model.value.noidi = makho;
  model.value.ngaylap = new Date();
};
////Data table
const loadLazyData = () => {
  loading.value = true;
  vattuApi.table(lazyParams.value).then((res) => {
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
const waiting = ref();
const excel = () => {
  waiting.value = true;
  vattuApi.excel(lazyParams.value).then((res) => {
    waiting.value = false;
    if (res.success) {
      window.open(res.link, "_blank");
    } else {
      alert(res.message);
    }
  });
};
watch(filters, async (newa, old) => {
  first.value = 0;
  loadLazyData();
});

watch(showing, async (newa, old) => {
  localStorage.setItem(column_cache, JSON.stringify(newa));
});
</script>
