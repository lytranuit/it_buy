<template>
  <div class="row my-5 align-items-end" v-if="model.is_dathang == true">

    <div class="col-4 form-group">
      <b class="">Ngày giao hàng dự kiến:</b>
      <div class="mt-2">
        <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false" showIcon
          :minDate="minDate" :readonly="readonly" @update:modelValue="changeNgaygiaohang" />
      </div>
    </div>

    <div class="col-12">
      <FormMuahangNhanhangVue></FormMuahangNhanhangVue>
    </div>
  </div>
</template>
<script setup>
import { computed, onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import FormMuahangNhanhangVue from "../Datatable/FormMuahangNhanhang.vue";
const store_muahang = useMuahang();
const { model } = storeToRefs(store_muahang);

const minDate = ref(new Date());

const changeNgaygiaohang = async () => {
  await muahangApi.save(model.value);
}
const readonly = computed(() => {
  if (model.value.date_finish)
    return true;
  return false;
})
onMounted(() => {

})
</script>

<style lang="scss"></style>
