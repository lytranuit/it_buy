<template>
    <div class="row clearfix">
        <div class="col-12">
            <h5 class="d-flex card-header drag-handle">
                <!-- <RouterLink to="/vattu/phieuxuat/add">
                    <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"></Button>
                </RouterLink> -->
                <div class="ml-auto">
                    <SelectButton v-model="type" :options="['Tổng hợp', 'Chi tiết']" aria-labelledby="basic"
                        :pt="{ button: 'p-button-sm' }" />
                </div>
            </h5>
            <section class="card card-fluid">
                <div class="card-body" style="overflow: auto; position: relative">
                    <div v-if="type == 'Tổng hợp'">
                        <TableTonghop></TableTonghop>
                    </div>
                    <div v-else>
                        <TableChitiet></TableChitiet>
                    </div>
                </div>
            </section>
        </div>

    </div>
</template>

<script setup>
import { onMounted, ref, computed, watch } from "vue";
import Button from "primevue/button";
import SelectButton from "primevue/selectbutton";
import TableTonghop from "../../../components/vattu/phieuxuat/TableTonghop.vue";
import TableChitiet from "../../../components/vattu/phieuxuat/TableChitiet.vue";

const type = ref("Tổng hợp");
onMounted(() => {
});
</script>
