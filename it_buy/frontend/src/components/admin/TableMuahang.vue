<template>
    <div class="col-lg-6">
        <div class="card">
            <div class="card-body">

                <div class="d-flex">
                    <div class="d-flex align-items-center">
                        <h4 class="header-title mb-0 mr-3">Mua hàng</h4>
                    </div>
                </div>
                <div class="mt-2">
                    <DataTable class="p-datatable-customers" showGridlines :value="datatable" :lazy="true" ref="dt"
                        :paginator="true" :rowsPerPageOptions="[10, 50, 100]" :rows="rows" :totalRecords="totalRecords"
                        @page="onPage($event)" :rowHover="true" responsiveLayout="scroll" :resizableColumns="true"
                        columnResizeMode="expand" v-model:filters="filters" filterDisplay="menu">
                        <template #empty>
                            <div class='text-center'>Không có dữ liệu.</div>
                        </template>
                        <Column v-for="(col, index) in columns" :field="col.data" :header="col.label" :key="col.data"
                            :showFilterMatchModes="false" :class="col.data" :className="col.className">

                            <template #body="slotProps">
                                <template v-if="col.data == 'name'">
                                    <div style="font-size: 12px;">
                                        <div>
                                            <RouterLink :to="'/muahang/edit/' + slotProps.data.id" class="text-blue">[{{
                        slotProps.data.code }}] {{ slotProps.data.name }}</RouterLink>
                                        </div>
                                        <div>Tạo bởi <i>{{ slotProps.data.user_created_by?.fullName }}</i> lúc {{
                        formatDate(slotProps.data.created_at, "YYYY-MM-DD HH:mm") }}</div>
                                    </div>
                                </template>
                                <template v-else-if="col.data == 'status_id'">
                                    <div class="text-center">
                                        <Tag value="Hoàn thành" severity="success"
                                            v-if="slotProps.data['date_finish']" />
                                        <Tag value="Chờ nhận hàng" severity="info"
                                            v-else-if="slotProps.data['is_dathang'] && ((slotProps.data['loaithanhtoan'] == 'tra_sau' && !slotProps.data['is_nhanhang']) || (slotProps.data['loaithanhtoan'] == 'tra_truoc' && slotProps.data['is_thanhtoan']))" />
                                        <Tag value="Chờ thanh toán" severity="info"
                                            v-else-if="slotProps.data['is_dathang'] && ((slotProps.data['loaithanhtoan'] == 'tra_truoc' && !slotProps.data['is_thanhtoan']) || (slotProps.data['loaithanhtoan'] == 'tra_sau' && slotProps.data['is_nhanhang']))" />
                                        <Tag value="Đang thực hiện" severity="secondary"
                                            v-else-if="slotProps.data[col.data] == 1" />
                                        <Tag value="Đang gửi và nhận báo giá" severity="warning"
                                            v-else-if="slotProps.data[col.data] == 6" />
                                        <Tag value="So sánh giá" severity="warning"
                                            v-else-if="slotProps.data[col.data] == 7" />
                                        <Tag value="Đang trình ký" severity="warning"
                                            v-else-if="slotProps.data[col.data] == 8" />
                                        <Tag value="Chờ ký duyệt" severity="warning"
                                            v-else-if="slotProps.data[col.data] == 9" />
                                        <Tag value="Đã duyệt" v-else-if="slotProps.data[col.data] == 10" />
                                        <Tag value="Không duyệt" severity="danger"
                                            v-else-if="slotProps.data[col.data] == 11" />
                                    </div>
                                </template>
                                <template v-else-if="col.data == 'tonggiatri'">
                                    {{ formatPrice(slotProps.data[col.data], 0) }} {{ slotProps.data['tiente'] }}
                                </template>
                                <template v-else>
                                    {{ slotProps.data[col.data] }}
                                </template>
                            </template>
                        </Column>
                    </DataTable>
                </div>
                <!--end /div-->
            </div>
            <!--end card-body-->
        </div>
        <!--end card-->
    </div>

</template>
<script setup>

import { onMounted, ref, computed } from "vue";
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Tag from 'primevue/tag';
import Api from "../../api/Api";
import { FilterMatchMode } from "primevue/api";
import { formatDate, formatPrice } from "../../utilities/util";

const datatable = ref();
const columns = ref([
    {
        label: "Tiêu đề",
        data: "name",
        className: "text-center",
        filter: true,
    },
    {
        label: "Trạng thái",
        data: "status_id",
        className: "text-center",
        filter: true,
    },
    {
        label: "Tổng giá trị",
        data: "tonggiatri",
        className: "text-center",
        filter: true,
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
const first = ref(0);
const rows = ref(10);
const draw = ref(0);
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
    Api.tablemuahang(lazyParams.value).then((res) => {
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

onMounted(() => {

    loadLazyData();
});
</script>