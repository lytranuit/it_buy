<template>
  <div class="row my-5">
    <div class="col-md-12 text-center">
      <a :href="model.pdf" :download="model.pdf" class="download-icon-link d-inline-flex align-items-center">
        <i class="far fa-file text-danger" style="font-size: 40px; margin-right: 10px;"></i>
        {{ model.pdf }}
      </a>
      <!-- <div class="d-inline-block ml-5" v-if="model.status_id == 8">
        <a :href="path_esign + '/admin/document/create?queue_id=' + model.esign_id">
          <Button label="Bắt đầu trình ký" class="p-button-primary p-button-sm mr-2"
            icon="fas fa-long-arrow-alt-right"></Button>
        </a>
      </div> -->

      <div class="d-inline-block ml-5" v-if="model.status_id == 9">
        <a :href="path_esign + '/admin/document/details/' + model.esign_id">
          <Button label="Đang trình ký" class="p-button-warning p-button-sm mr-2"
            icon="fas fa-spinner fa-spin"></Button>
        </a>
        <a :href="path_esign + '/admin/document/edit/' + model.esign_id">
          <Button label="Đi đến cài đặt" class="p-button-sm mr-2" icon="fas fa-cog"></Button>
        </a>
      </div>

      <div class="d-inline-block ml-5" v-else-if="model.status_id == 10">
        <a :href="path_esign + '/admin/document/details/' + model.esign_id">
          <Button label="Đã hoàn thành" class="p-button-success p-button-sm mr-2" icon="fas fa-thumbs-up"></Button>
        </a>
      </div>

      <div class="d-inline-block ml-5" v-else-if="model.status_id == 11">
        <a :href="path_esign + '/admin/document/details/' + model.esign_id">
          <Button label="Không duyệt" class="p-button-danger p-button-sm mr-2" icon="fas fa-times"></Button>
        </a>
      </div>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import Button from 'primevue/button';
const store_muahang = useMuahang();
const { model } = storeToRefs(store_muahang);

const path_esign = import.meta.env.VITE_ESIGNURL;
onMounted(() => {

})
</script>

<style lang="scss"></style>
