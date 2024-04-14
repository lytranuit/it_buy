<template>
  <div class="row clearfix">
    <div class="col-12 mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Dự trù' },]">
      </Breadcrumb>
    </div>
    <div class="col-12">
      <TableDutru></TableDutru>
    </div>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableDutru from '../../components/dutru/TableDutru.vue';
</script>
