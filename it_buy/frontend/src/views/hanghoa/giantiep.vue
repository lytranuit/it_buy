<template>
  <div id="TableDutruChitiet">
    <div class="mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Hàng hóa' }, { label: 'Mua hàng gián tiếp' },]">
      </Breadcrumb>
    </div>
    <TableHanghoa :type="2"></TableHanghoa>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableHanghoa from '../../components/hanghoa/TableHanghoa.vue';

</script>
