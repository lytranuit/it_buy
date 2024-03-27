<template>
    <div class="row justify-content-center">
        <div class="col-md-6 col-lg-3">
            <div class="card report-card bg-purple-gradient shadow-purple">
                <div class="card-body">
                    <div class="float-right">
                        <i class="dripicons-wallet report-main-icon bg-icon-purple"></i>
                    </div>
                    <span class="badge badge-light text-purple">Dự trù</span>
                    <h3 class="my-3">{{ sodutru }}</h3>
                </div>
                <!--end card-body-->
            </div>
            <!--end card-->
        </div>
        <!--end col-->
        <div class="col-md-6 col-lg-3">
            <div class="card report-card bg-warning-gradient shadow-warning">
                <div class="card-body">
                    <div class="float-right">
                        <i class="fas fa-spinner report-main-icon bg-icon-warning"></i>
                    </div>
                    <span class="badge badge-light text-warning">Đề nghị mua hàng</span>
                    <h3 class="my-3">{{ somuahang }}</h3>
                </div>
                <!--end card-body-->
            </div>
            <!--end card-->
        </div>
        <!--end col-->
        <div class="col-md-6 col-lg-3">
            <div class="card report-card bg-success-gradient shadow-success">
                <div class="card-body">
                    <div class="float-right">
                        <i class="dripicons-checkmark report-main-icon bg-icon-success"></i>
                    </div>
                    <span class="badge badge-light text-success">Đã hoàn thành</span>
                    <h3 class="my-3">{{ success }}</h3>
                </div>
                <!--end card-body-->
            </div>
            <!--end card-->
        </div>
        <!--end col-->

        <div class="col-md-6 col-lg-3">
            <div class="card report-card bg-danger-gradient shadow-danger">
                <div class="card-body">
                    <div class="float-right">
                        <i class="fas fa-ban report-main-icon bg-icon-danger"></i>
                    </div>
                    <span class="badge badge-light text-danger">Đã thất bại</span>
                    <h3 class="my-3">{{ failed }}</h3>
                </div>
                <!--end card-body-->
            </div>
            <!--end card-->
        </div>
        <!--end col-->
        
        <ChartChiphi></ChartChiphi>
        <Chartbophan></Chartbophan>
        <TableDutru></TableDutru>
        <TableMuahang></TableMuahang>
    </div>
    <Loading :waiting="waiting"></Loading>
</template>
<script setup>
import { onMounted, ref } from "vue";
import Api from "../../api/Api";
import Loading from "../../components/Loading.vue";
import { useAuth } from "../../stores/auth";
import { useRouter } from "vue-router";
import Chartbophan from "../../components/admin/Chartbophan.vue";
import TableDutru from "../../components/admin/TableDutru.vue";
import ChartChiphi from "../../components/admin/ChartChiphi.vue";
import TableMuahang from "../../components/admin/TableMuahang.vue";
const store = useAuth();
const sodutru = ref(0);
const somuahang = ref(0);
const success = ref(0);
const failed = ref(0);
const waiting = ref(false);

const router = useRouter();

// const now = new Date();
onMounted(() => {
    Api.HomeBadge().then((res) => {
        sodutru.value = res.sodutru;
        somuahang.value = res.somuahang;
        success.value = res.success;
        failed.value = res.failed;
    });
});
</script>