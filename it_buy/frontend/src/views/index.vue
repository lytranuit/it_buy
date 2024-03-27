<template>
    
</template>
<script setup>
import { onMounted, ref } from "vue";
import Chart from "primevue/chart";
import Api from "../api/Api";
import Loading from "../components/Loading.vue";
import { useAuth } from "../stores/auth";
import { useRouter } from "vue-router";
import SelectButton from 'primevue/selectbutton';
import Calendar from 'primevue/calendar';
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
    Api.HomeBadge().then((res) => {
        sodutru.value = res.sodutru;
        somuahang.value = res.somuahang;
        success.value = res.success;
        failed.value = res.failed;
    });

});
</script>