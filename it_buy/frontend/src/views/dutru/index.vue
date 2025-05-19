<template>
  <div class="row clearfix">
    <div class="col-12 mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Dự trù' },]">
      </Breadcrumb>
    </div>
    <div class="col-12">
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <div class="card-header d-md-flex align-items-center">
            <PopupDutru></PopupDutru>
            <!-- <div class="ml-auto">
              <SelectButton v-model="type" :options="['Dự trù', 'Chi tiết']" aria-labelledby="basic"
                :pt="{ button: 'p-button-sm' }" />
            </div> -->
          </div>
          <div class="card-body">
            <div v-if="type == 'Dự trù'">
              <TableDutru></TableDutru>
            </div>
            <div v-else>
              <TableDutruChitiet></TableDutruChitiet>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableDutru from '../../components/dutru/TableDutru.vue';
import SelectButton from 'primevue/selectbutton';
import PopupDutru from '../../components/dutru/PopupDutru.vue';
import { ref } from 'vue';
import TableDutruChitiet from '../../components/dutru/TableDutruChitiet.vue';
const type = ref('Dự trù');
</script>
