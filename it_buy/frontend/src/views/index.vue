<template>
    <div class="row justify-content-center">

        <div class="col-md-12">
            <div class="card">
                <div class="card-header">
                    <b>Quy trình mua hàng</b>
                </div>
                <div class="card-body">
                    <Stepper>
                        <StepperPanel :header="item.label" v-for="item in items">
                        </StepperPanel>
                    </Stepper>
                </div>
                <!--end card-body-->
            </div>
            <!--end card-->
        </div>
        <!--end col-->
        <!-- <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <b>Thảo luận liên quan đến tôi</b>
                </div>
                <div class="card-body">

                </div>
              
            </div>
           
        </div> -->
        <!--end col-->
    </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import Chart from "primevue/chart";
import Stepper from 'primevue/stepper';
import StepperPanel from 'primevue/stepperpanel';
import Api from "../api/Api";
import Loading from "../components/Loading.vue";
import { useAuth } from "../stores/auth";
import { useRouter } from "vue-router";
import SelectButton from 'primevue/selectbutton';
import Calendar from 'primevue/calendar';
const items = ref([{
    label: 'Dự trù'
}, {
    label: 'Trình ký'
}, {
    label: 'Tiếp nhận'
}, {
    label: 'Đề nghị mua hàng'
}, {
    label: 'Trình ký'
}, {
    label: 'Đặt hàng'
}, {
    label: 'Nhận hàng và thanh toán'
}, {
    label: 'Hoàn thành'
}])
const store = useAuth();
const sodutru = ref(0);
const somuahang = ref(0);
const success = ref(0);
const failed = ref(0);
const waiting = ref(false);
const chartOptions = ref({
    responsive: true,
    plugins: {
        legend: {
            display: false,
        },
    },
});
const chartData = ref({
    labels: [],
    datasets: [],
});
const router = useRouter();
const dates = ref();
const type_chiphi = ref("Month");
onMounted(() => {
    // Api.comments({}).then((res) => {
    //     console.log(res);
    // })
});
</script>