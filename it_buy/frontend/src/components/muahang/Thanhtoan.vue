<template>
  <div class="row my-5">
    <div class="col-md-12 text-center">
      <h3>Kiểm tra thông tin</h3>
      <a href="#" @click.prevent="tabviewActive = 1">-> Files</a>
    </div>
    <div class="col-md-12 mt-3" v-if="model.is_dathang == true && !model.is_thanhtoan">
      <Button label="Chấp nhận thanh toán" icon="fas fa-long-arrow-alt-down" class="p-button-success p-button-sm mr-2"
        @click.prevent="thanhtoan()">
      </Button>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import Button from 'primevue/button';
const store_muahang = useMuahang();
const { model, tabviewActive } = storeToRefs(store_muahang);

const emit = defineEmits(["next"]);

const thanhtoan = async () => {
  model.value.is_thanhtoan = true;
  await muahangApi.save(model.value);
  emit("next");
}
onMounted(() => {

})
</script>

<style lang="scss"></style>
