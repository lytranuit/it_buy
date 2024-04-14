<template>
  <div id="TableDutruChitiet">
    <div class="mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Hàng hóa' }]">
      </Breadcrumb>
    </div>
    <TableHanghoa></TableHanghoa>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableHanghoa from '../../components/hanghoa/TableHanghoa.vue';

</script>
