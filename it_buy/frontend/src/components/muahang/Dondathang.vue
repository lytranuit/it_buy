<template>
  <div class="row my-5">
    <div class="row col-12">
      <div class="col-md-4">
        <b class="">Loại thanh toán:</b>
        <div class="mt-2">
          <select class="form-control form-control-sm" v-model="model.loaithanhtoan" :disabled="model.is_dathang">
            <option value="tra_truoc">Trả trước</option>
            <option value="tra_sau">Trả sau</option>
          </select>
        </div>
      </div>
      <div class="col-md-4">
        <b class="">Phương thức thanh toán:</b>
        <div class="mt-2">
          <input v-model="model.ptthanhtoan" class="form-control form-control-sm" :disabled="model.is_dathang" />
        </div>
      </div>
      <div class="col-md-4">
        <b class="">Yêu cầu giao hàng:</b>
        <div class="mt-2">
          <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false" showIcon
            :minDate="minDate" :disabled="model.is_dathang" />
        </div>
      </div>
      <div class="col-md-12 mt-2">
        <b class="">Địa chỉ giao hàng:</b>
        <div class="mt-2">
          <textarea v-model="model.diachigiaohang" class="form-control form-control-sm"
            :disabled="model.is_dathang"></textarea>
        </div>
      </div>

    </div>
    <div class="col-md-12 text-center mt-3" v-if="!model.is_dathang">
      <Button label="Xuất đơn đặt hàng" icon="pi pi-file" class="p-button p-button-sm mr-2"
        @click.prevent="xuatdondathang()"></Button>
    </div>
    <div class="col-md-12 mt-3 file-box-content" v-if="model.dondathang">
      <div class="file-box">
        <a :href="model.dondathang" :download="download(model.dondathang)" class="download-icon-link" target="_blank">
          <i class="dripicons-download file-download-icon"></i>

          <div class="text-center">
            <i class="far fa-file-pdf text-danger"></i>
            <h6 class="text-truncate" title="Đơn đặt hàng">Đơn đặt hàng</h6>
          </div>
        </a>
      </div>
    </div>
    <div class="col-md-12 mt-3" v-if="!model.is_dathang">
      <Button label="Hoàn thành đặt hàng" icon="fas fa-long-arrow-alt-down" class="p-button-success p-button-sm mr-2"
        @click.prevent="dathang()"></Button>
    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import Button from 'primevue/button';
import { useToast } from "primevue/usetoast";
import { download } from "../../utilities/util";
const toast = useToast();
const store_muahang = useMuahang();
const { model, waiting } = storeToRefs(store_muahang);

const emit = defineEmits(["next"]);
const minDate = ref(new Date());

const xuatdondathang = async () => {
  if (!model.value.loaithanhtoan) {
    alert("Chưa chọn phương thức thanh toán!");
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn hạn giao hàng!");
    return false;
  }
  waiting.value = true;
  await muahangApi.save(model.value);
  muahangApi.xuatdondathang(model.value.id).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
      // location.reload();
      store_muahang.load_data(model.value.id);
    }
  });
}
const dathang = async () => {
  if (!model.value.loaithanhtoan) {
    alert("Chưa chọn phương thức thanh toán!");
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn hạn giao hàng!");
    return false;
  }
  // console.log(model.value);
  waiting.value = true;
  model.value.is_dathang = true;
  await muahangApi.save(model.value);
  waiting.value = false;
  emit("next");
}
onMounted(() => {

})
</script>

<style lang="scss"></style>
