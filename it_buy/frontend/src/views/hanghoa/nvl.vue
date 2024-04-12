<template>
  <div id="TableDutruChitiet">
    <div class="mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Hàng hóa' }, { label: 'Nguyên vật liệu' },]">
      </Breadcrumb>
    </div>
    <TableHanghoa :type="1"></TableHanghoa>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableHanghoa from '../../components/hanghoa/TableHanghoa.vue';

</script>