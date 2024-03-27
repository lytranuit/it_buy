<template>
    <div class="col-lg-8">
        <div class="card">
            <div class="card-body">

                <div class="d-flex">
                    <div class="d-flex align-items-center">
                        <h4 class="header-title mb-0 mr-3">Chi phí</h4>
                        <Calendar v-model="dates" selectionMode="range" :manualInput="false"
                            inputClass="form-control-sm" @hide="changeDate" dateFormat="yy/mm/dd" />
                    </div>
                    <SelectButton v-model="type_chiphi" :options="['Day', 'Week', 'Month', 'Year']"
                        aria-labelledby="basic" class="ml-auto" :pt="{ 'button': 'form-control-sm' }"
                        @change="changeDate" />
                </div>
                <div class="">
                    <Chart v-bind="chart1" />
                </div>
                <!--end /div-->
            </div>
            <!--end card-body-->
        </div>
        <!--end card-->
    </div>

</template>
<script setup>

import { onMounted, ref } from "vue";
import Chart from "primevue/chart";
import moment from "moment";
import Api from "../../api/Api";
import SelectButton from 'primevue/selectbutton';
import Calendar from 'primevue/calendar';

const chart1 = ref({
    type: "bar",
    options: {
        maintainAspectRatio: false,
        aspectRatio: 0.6,
        plugins: {
            legend: {
                labels: {
                    color: '#334155'
                }
            },
        },
        scales: {
            x: {
                ticks: {
                    color: '#334155'
                },
                grid: {
                    color: '#e2e8f0'
                }
            },
            y: {
                ticks: {
                    color: '#334155'
                },
                grid: {
                    color: '#e2e8f0'
                }
            }
        }
    },
    height: 300,
    class: "h-30rem"
});
const dates = ref([moment().subtract(3, "months").toDate(), new Date()]);
const type_chiphi = ref("Month");
const changeDate = () => {
    load_chiphi();
}

const load_chiphi = () => {
    Api.chiphi(dates.value[0], dates.value[1], type_chiphi.value).then((res) => {
        chart1.value.data = res.data;
    });
}
onMounted(() => {
    load_chiphi();
});
</script>