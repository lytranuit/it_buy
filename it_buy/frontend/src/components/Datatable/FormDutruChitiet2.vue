<template>
    <div id="TableDutruChitiet">
        <DataTable class="p-datatable-customers" showGridlines :value="datatable1" :lazy="true" ref="dt"
            scrollHeight="70vh" v-model:selection="selected" :paginator="true" :rowsPerPageOptions="[10, 20, 50, 100]"
            :rows="rows" :totalRecords="totalRecords" @page="onPage($event)" :rowHover="true" :loading="loading"
            responsiveLayout="scroll" :resizableColumns="true" columnResizeMode="expand" v-model:filters="filters"
            filterDisplay="menu">
            <template #header>
                <Button label="Tạo đề nghị mua hàng" icon="pi pi-plus" class="p-button-success p-button-sm"
                    :disabled="!selected || !selected.length" @click="taodenghimuahang()"></Button>
                <div class="d-inline-flex float-right">
                    <SelectButton v-model="filterTable" :options="['Đã xử lý', 'Chưa xử lý']" aria-labelledby="basic"
                        :pt="{ 'button': 'form-control-sm' }" @change="loadLazyData" />

                    <!-- <Button label="Chi tiết" class="p-button-primary p-button-sm" @click="chitiet()"
              v-if="type == 'tonghop'"></Button>
            <Button label="Tổng hợp" class="p-button-warning p-button-sm" @click="tonghop()"
              v-else-if="type == 'chitiet'"></Button> -->
                </div>
            </template>

            <template #empty>
                <div class='text-center'>Không có dữ liệu.</div>
            </template>
            <Column selectionMode="multiple"></Column>
            <Column v-for="(col, index) in selectedColumns" :field="col.data" :header="col.label" :key="col.data"
                :showFilterMatchModes="false">

                <template #body="slotProps">
                    <template v-if="col.data == 'list_dutru'">
                        <div v-for="item of slotProps.data[col.data]" :key="item.id">
                            <RouterLink :to="'/dutru/edit/' + item.id" class="text-primary">{{ item.code }}</RouterLink>
                        </div>
                    </template>

                    <template v-else-if="col.data == 'list_muahang'">
                        <div v-for="item of slotProps.data[col.data]" :key="item.id">
                            <RouterLink :to="'/muahang/edit/' + item.id" class="text-primary mr-2">{{ item.id }} - {{
            item.code }}
                            </RouterLink>
                            <Badge value="Hoàn thành" size="small" severity="success" v-if="item['date_finish']" />
                            <Badge value="Chờ nhận hàng & thanh toán" size="small" severity="warning"
                                v-else-if="item['is_dathang']" />
                            <Badge value="Đang thực hiện" size="small" severity="warning"
                                v-else-if="item['status_id'] == 1" />
                            <Badge value="Đang gửi và nhận báo giá" size="small" severity="warning"
                                v-else-if="item['status_id'] == 6" />
                            <Badge value="So sánh giá" size="small" severity="warning"
                                v-else-if="item['status_id'] == 7" />
                            <Badge value="Đang trình ký" size="small" v-else-if="item['status_id'] == 8" />
                            <Badge value="Chờ ký duyệt" size="small" severity="warning"
                                v-else-if="item['status_id'] == 9" />
                            <Badge value="Đã duyệt" size="small" severity="success"
                                v-else-if="item['status_id'] == 10" />
                            <Badge value="Không duyệt" size="small" severity="danger"
                                v-else-if="item['status_id'] == 11" />
                        </div>
                    </template>

                    <template v-else-if="col.data == 'ngayhethan'">
                        <div v-for="item of slotProps.data['list_dutru']" :key="item.id">
                            {{ formatDate(item.date) }}
                        </div>
                    </template>

                    <template v-else-if="col.data == 'thanhtien'">
                        {{ formatPrice(slotProps.data[col.data], 0) }} VNĐ
                    </template>

                    <template v-else-if="col.data == 'soluong_dutru'">
                        {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
                    </template>

                    <template v-else-if="col.data == 'soluong_mua'">
                        {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
                    </template>

                    <template v-else-if="col.data == 'soluong'">
                        {{ slotProps.data[col.data] }} {{ slotProps.data["dvt"] }}
                    </template>
                    <template v-else-if="col.data == 'mahh'">
                        {{ slotProps.data[col.data] }} <i class="fas fa-sync-alt" style="cursor: pointer;"
                            @click="openNew(slotProps.data)"></i>
                    </template>
                    <template v-else>
                        {{ slotProps.data[col.data] }}
                    </template>
                </template>

                <template #filter="{ filterModel, filterCallback }" v-if="col.filter == true">
                    <InputText type="text" v-model="filterModel.value" @keydown.enter="filterCallback()"
                        class="p-column-filter" />
                </template>
            </Column>
        </DataTable>
        <Dialog v-model:visible="visibleDialog" header="Đổi mã hàng hóa" :modal="true" style="width: 75vw;"
            :breakpoints="{ '1199px': '75vw', '575px': '95vw' }">
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
                        <MaterialTreeSelect v-model="modelDialog.to_id" :useID="false"
                            @update:model-value="changeMahh()">
                        </MaterialTreeSelect>
                    </div>
                </div>
                <div class="field col">
                    <label for="name">Thông báo đến người dự trù</label>
                    <div>
                        <Button label="Thông báo" icon="far fa-paper-plane" size="small" class="mr-2"
                            @click="thongbaodoima"></Button>
                        <Button label="Lưu lại" icon="pi pi-check" size="small" severity="success"
                            @click="savedoima"></Button>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-12">
                    Lịch sử mua hàng của mã {{ modelDialog.to }}
                </div>
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
                            <tr v-for="(item, index) in modelDialog.history">
                                <th>
                                    <RouterLink :to="'/muahang/edit/' + item.muahang.id">{{ item.muahang.code }} - {{
            item.muahang.name }}
                                    </RouterLink>
                                </th>
                                <th>{{ item.tenhh }}</th>
                                <th>{{ item.soluong }}</th>
                                <th>{{ formatPrice(item.dongia, 0) }} VND</th>
                                <th>{{ formatPrice(item.thanhtien, 0) }} VND</th>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </Dialog>
    </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import dutruApi from "../../api/dutruApi";
import Badge from "primevue/badge";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import DataTable from "primevue/datatable";
import { FilterMatchMode } from "primevue/api";
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
const filterTable = ref();
const toast = useToast();
const store_muahang = useMuahang();
const { datatable } = storeToRefs(store_muahang);
const route = useRoute();
const router = useRouter();
const datatable1 = ref();
const selected = ref();
const columns = ref([
    {
        id: 0,
        label: "Mã",
        data: "mahh",
        className: "text-center",
    },

    {
        id: 1,
        label: "Tên",
        data: "tenhh",
        className: "text-center",
    },

    {
        id: 3,
        label: "Số lượng dự trù",
        data: "soluong_dutru",
        className: "text-center",
    },

    {
        id: 4,
        label: "Mã dự trù",
        data: "list_dutru",
        className: "text-center",
    },
    {
        id: 5,
        label: "Hạn giao hàng",
        data: "ngayhethan",
        className: "text-center",
    },

    {
        id: 6,
        label: "Đề nghị mua hàng",
        data: "list_muahang",
        className: "text-center",
    },
    {
        id: 7,
        label: "Số lượng mua",
        data: "soluong_mua",
        className: "text-center",
    },
    {
        id: 8,
        label: "Còn lại",
        data: "soluong",
        className: "text-center",
    },
    {
        id: 9,
        label: "Thành tiền",
        data: "thanhtien",
        className: "text-center",
    },
]);
const filters = ref({
    id: { value: null, matchMode: FilterMatchMode.CONTAINS },
    mahh: { value: null, matchMode: FilterMatchMode.CONTAINS },
    tenhh: { value: null, matchMode: FilterMatchMode.CONTAINS },
    list_dutru: { value: null, matchMode: FilterMatchMode.CONTAINS },
});
const totalRecords = ref(0);
const loading = ref(true);
const showing = ref([]);
const column_cache = "columns_dutru_chitiet"; ////
const first = ref(0);
const rows = ref(20);
const draw = ref(0);
const selectedColumns = computed(() => {
    return columns.value.filter((col) => showing.value.includes(col.id));
});
const lazyParams = computed(() => {
    let data_filters = {};
    for (let key in filters.value) {
        data_filters[key] = filters.value[key].value;
    }
    let order_by = {};
    order_by['id'] = "Asc";
    return {
        draw: draw.value,
        start: first.value,
        length: rows.value,
        filters: data_filters,
        dutru_id: route.params.id,
        filterTable: filterTable.value,
        orderBy: order_by
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
    if (selected.value.length) {
        for (var item of selected.value) {
            if (!(item.soluong > 0)) {
                alert("Hàng hóa đã mua đủ số lượng!");
                return false;
            }
        }
    }
    datatable.value = selected.value.map((item, key) => {

        item.type_id = item.list_dutru[0].type_id
        item.dvt_dutru = item.dvt;
        item.quidoi = 1;
        item.soluong_dutru = item.soluong;
        item.stt = key + 1;
        item.ids = rand();
        delete item.list_dutru;
        delete item.list_muahang;
        delete item.id;
        return item;
    });
    router.push("/muahang/add?no_reset=1");
}

const visibleDialog = ref();
const modelDialog = ref({});
const openNew = async (row) => {
    visibleDialog.value = true;
    modelDialog.value.id = row.id;
    modelDialog.value.from_hh = row.mahh;
    modelDialog.value.to_id = row.hh_id;
    changeMahh();
}
const savedoima = async () => {
    visibleDialog.value = false;
    var res = await dutruApi.savedoima({ dutru_chitiet_id: modelDialog.value.id, hh_id: modelDialog.value.to_id });
    if (res.success) {
        toast.add({
            severity: "success",
            summary: "Thành công",
            detail: "Thành công",
            life: 3000,
        });
        loadLazyData();
    }
}
const thongbaodoima = async () => {
    // visibleDialog.value = false;
    var res = await dutruApi.thongbaodoima({ dutru_chitiet_id: modelDialog.value.id, hh_id: modelDialog.value.to_id });
    if (res.success) {
        toast.add({
            severity: "success",
            summary: "Thành công",
            detail: "Thành công",
            life: 3000,
        });
    }
}
const changeMahh = async () => {

    var res = await muahangApi.getHistory(modelDialog.value.to_id);
    // console.log(res);
    modelDialog.value.to = res.to;
    modelDialog.value.history = res.data;
}
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