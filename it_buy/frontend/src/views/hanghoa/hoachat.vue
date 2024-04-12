<template>
  <div id="TableDutruChitiet">
    <div class="mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Hàng hóa' }, { label: 'Hóa chất, thuốc thử QC' },]">
      </Breadcrumb>
    </div>
    <TableHanghoa :type="3"></TableHanghoa>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableHanghoa from '../../components/hanghoa/TableHanghoa.vue';

</script>