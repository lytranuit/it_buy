<template>
    <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt" scrollHeight="70vh"
        v-model:selection="selectedProducts" :paginator="true" :rowsPerPageOptions="[10, 50, 100]" :rows="rows"
        :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading"
        responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters"
        filterDisplay="menu" editMode="cell" @cell-edit-complete="onCellEditComplete">
        <template #header>
            <div style="width: 200px">
                <TreeSelect :options="columns" v-model="showing" multiple :limit="0" :normalizer="normalizer"
                    :limitText="(count) => 'Hiển thị: ' + count + ' cột'">
                </TreeSelect>
            </div>
        </template>

        <template #empty> Không có dữ liệu. </template>
        <Column v-for="col of selectedColumns" :field="col.data" :header="col.label" :key="col.data"
            :showFilterMatchModes="false">
            <template #body="slotProps">
                <template v-if="col.data == 'sohd' && user.email == slotProps.data.hoadon.created_by">
                    <RouterLink :to="'/vattu/phieuxuat/edit/' + slotProps.data.hoadon.id">
                        <i class="fas fa-pencil-alt mr-2"></i>
                        {{ slotProps.data[col.data] }}
                    </RouterLink>
                </template>
                <template v-else-if="col.data == 'ngaylap'">
                    {{ formatDate(slotProps.data.hoadon[col.data]) }}
                </template>
                <template v-else-if="col.data == 'soluong'">
                    {{ formatPrice(slotProps.data[col.data]) }} {{ slotProps.data.dvt }}
                </template>
                <template v-else-if="col.data == 'noidi'">
                    {{ slotProps.data.hoadon[col.data] }}
                </template>
                <template v-else-if="col.data == 'noiden'">
                    {{ slotProps.data.hoadon[col.data] }}
                </template>
                <template v-else-if="col.data == 'kt_xuat'">
                    <i class="pi true-icon pi-check-circle text-success" v-if="slotProps.data[col.data]"></i>
                    <i class="pi false-icon pi-times-circle text-danger" v-else></i>
                </template>
                <template v-else-if="col.data == 'kt_nhap'">
                    <i class="pi true-icon pi-check-circle text-success" v-if="slotProps.data[col.data]"></i>
                    <i class="pi false-icon pi-times-circle text-danger" v-else></i>
                </template>
                <div v-else v-html="slotProps.data[col.data]"></div>
            </template>
            <template #editor="{ data, field }" v-if="col.data == 'kt_nhap'">
                <div class="custom-control custom-switch switch-success">
                    <input type="checkbox" class="custom-control-input" id="ICOnotify" v-model="data[field]" />
                    <label class="custom-control-label" for="ICOnotify"></label>
                </div>
            </template>
            <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                    class="p-column-filter" />
            </template>
        </Column>
    </DataTable>
</template>
<script setup>
import { onMounted, ref, computed, watch } from "vue";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
import Column from "primevue/column";
import InputText from "primevue/inputtext";
import { useConfirm } from "primevue/useconfirm";
import VattuApi from "../../../api/VattuApi";
import { formatDate, formatPrice } from "../../../utilities/util";
import { useToast } from "primevue/usetoast";
import { useAuth } from "../../../stores/auth";
import { storeToRefs } from "pinia";

const store_auth = useAuth();
const { user } = storeToRefs(store_auth);
const toast = useToast();
const confirm = useConfirm();
const datatable = ref();
const columns = ref([
    {
        label: "Mã phiếu",
        data: "sohd",
        className: "text-center",
        filter: true,
    },
    {
        label: "Ngày lập",
        data: "ngaylap",
        className: "text-center",
    },
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
    },
    {
        label: "Số lượng",
        data: "soluong",
        className: "text-center",
    },
    {
        label: "Từ kho",
        data: "noidi",
        className: "text-center",
    },
    {
        label: "Xác nhận xuất",
        data: "kt_xuat",
        className: "text-center",
    },
    {
        label: "Người xuất",
        data: "user_xuat",
        className: "text-center",
    },
    {
        label: "Đến kho",
        data: "noiden",
        className: "text-center",
    },
    {
        label: "Xác nhận nhận",
        data: "kt_nhap",
        className: "text-center",
    },
    {
        label: "Người nhận",
        data: "user_nhap",
        className: "text-center",
    },
]);
const filters = ref({});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_phieuxuat_chitiet";
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

const onCellEditComplete = (event) => {
    let { newValue, value, newData, index, field, data } = event;
    if (newValue == value) {
        return false;
    }
    if (field == "kt_nhap") {
        var data1 = {};
        data1.id = newData.id;
        data1.kt_nhap = newData.kt_nhap;
        VattuApi.xacnhanNhap(data1).then((res) => {
            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Thay đổi thành công",
                life: 3000,
            });
            loadLazyData();
        });
    }

    datatable.value[index] = newData;



};
// Data table
const loadLazyData = () => {
    loading.value = true;
    VattuApi.tableXuatChitiet(lazyParams.value).then((res) => {
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
        message: "Bạn có muốn xóa phiếu nhập này?",
        header: "Xác nhận",
        icon: "pi pi-exclamation-triangle",
        accept: () => {
            VattuApi.removeXuat({ id: id }).then((res) => {
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
    for (var col of columns.value) {
        if (!col.filter) continue;
        filters.value[col.data] = {
            value: null,
            matchMode: FilterMatchMode.CONTAINS,
        };
    }
    loadLazyData();
});
watch(filters, async (newa, old) => {
    first.value = 0;
    loadLazyData();
});
watch(showing, (newa, old) => {
    localStorage.setItem(column_cache, JSON.stringify(newa));
});
</script>
