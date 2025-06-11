<template>
  <div class="row clearfix">
    <div class="col-12 mb-3">
      <Breadcrumb :home="{ icon: 'pi pi-home' }" :model="[{ label: 'Đề nghị xuất vật tư' },]">
      </Breadcrumb>
    </div>
    <div class="col-12">
      <section class="card card-fluid">
        <div class="card-body" style="overflow: auto; position: relative">
          <div class="card-header d-md-flex align-items-center">
            <!-- <PopupXuat></PopupDutru> -->
            <!-- <div class="ml-auto">
              <SelectButton v-model="type" :options="['Dự trù', 'Chi tiết']" aria-labelledby="basic"
                :pt="{ button: 'p-button-sm' }" />
            </div> -->
            <Button label="Tạo mới" icon="pi pi-plus" size="small" severity="success" class="mr-2"
              @click="createNew"></Button>
          </div>
          <div class="card-body">
            <TableXuatVattu></TableXuatVattu>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import Breadcrumb from 'primevue/breadcrumb';
import TableXuatVattu from '../../components/xuatvattu/TableXuatVattu.vue';
import Button from 'primevue/button';
import { useRouter } from 'vue-router';
const router = useRouter();
const createNew = () => {
  router.push("/xuatvattu/add");
};
</script>
