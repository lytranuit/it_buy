<template>
  <div class="row clearfix">
    <div class="col-12 mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Dự trù' }, { label: 'Mua hàng gián tiếp' },]">
      </Breadcrumb>
    </div>
    <div class="col-12">
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
         <TableDutru :type="2"></TableDutru>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableDutru from '../../components/dutru/TableDutru.vue';
</script>
