<template>
    <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt" scrollHeight="70vh"
        v-model:selection="selectedProducts" :paginator="true" :rowsPerPageOptions="[10, 50, 100]" :rows="rows"
        :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading"
        responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters"
        filterDisplay="menu">
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
                <template v-if="col.data == 'id'">
                    <RouterLink :to="'/vattu/phieunhap/edit/' + slotProps.data[col.data]">
                        <i class="fas fa-pencil-alt mr-2"></i>
                        {{ slotProps.data[col.data] }}
                    </RouterLink>
                </template>
                <template v-else-if="col.data == 'ngaylap'">
                    {{ formatDate(slotProps.data[col.data]) }}
                </template>
                <template v-else-if="col.data == 'user_created'">
                    <span v-if="slotProps.data.user_created">{{ slotProps.data.user_created.FullName
                    }}</span>
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
                <a class="p-link text-danger font-16" @click="confirmDelete(slotProps.data['id'])"><i
                        class="pi pi-trash"></i></a>
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
import { formatDate } from "../../../utilities/util";

const confirm = useConfirm();
const datatable = ref();
const columns = ref([
    {
        label: "ID",
        data: "id",
        className: "text-center",
        filter: true,
    },
    {
        label: "Mã phiếu",
        data: "sohd",
        className: "text-center",
        filter: true,
    },
    {
        label: "Ngày nhập",
        data: "ngaylap",
        className: "text-center",
    },
    {
        label: "Người nhập",
        data: "user_created",
        className: "text-center",
        filter: true,
    },
    {
        label: "Ghi chú",
        data: "ghichu",
        className: "text-center",
    },
    {
        label: "Mã kho",
        data: "makho",
        className: "text-center",
    }
]);
const filters = ref({});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_phieunhap";
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

// Data table
const loadLazyData = () => {
    loading.value = true;
    VattuApi.tableNhap(lazyParams.value).then((res) => {
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
            VattuApi.removeNhap({ id: id }).then((res) => {
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
