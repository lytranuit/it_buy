<template>
  <div class="card-body">
    <div class="row">
      <div class="col-md-9">
        <div class="form-group row">
          <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
          <div class="col-12 col-lg-12 pt-1">
            <input class="form-control" v-model="model.name" required :disabled="readonly">
          </div>
        </div>
      </div>
      <div class="col-md-12">
        <div class="form-group row">
          <b class="col-12 col-lg-12 col-form-label">Lý do mua hàng:<i class="text-danger">*</i></b>
          <div class="col-12 col-lg-12 pt-1">
            <textarea class="form-control" v-model="model.note" required :disabled="readonly"></textarea>
          </div>
        </div>
      </div>
      <div class="col-md-12">
        <div class="form-group row">
          <b class="col-12 col-lg-12 col-form-label">Hàng hóa:<i class="text-danger">*</i></b>
          <div class="col-12 col-lg-12 pt-1">
            <FormMuahangChitiet></FormMuahangChitiet>
          </div>
        </div>
        <!-- <div class="form-group row">
                        <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                        <div class="col-12 col-lg-12 pt-1">
                          <textarea class="form-control form-control-sm" v-model="model.note"
                            :disabled="model.status_id != 1"></textarea>
                        </div>
                      </div> -->
      </div>
      <div class="col-md-12 text-center" v-if="model.status_id == 1">
        <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
          @click.prevent="submit()"></Button>
        <Button label="Gửi và nhận báo giá" icon="far fa-paper-plane" class="p-button-sm mr-2"
          @click.prevent="baogia()"></Button>
      </div>

    </div>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import Api from "../../api/Api";
import { formatDate } from "../../utilities/util";
import { useMuahang } from "../../stores/muahang";
import { storeToRefs } from "pinia";
import muahangApi from "../../api/muahangApi";
import InputGroup from 'primevue/inputgroup';
import Textarea from 'primevue/textarea';
import Button from 'primevue/button';
import { useToast } from "primevue/usetoast";
import FormMuahangChitiet from "../Datatable/FormMuahangChitiet.vue";
const toast = useToast();
const store_muahang = useMuahang();
const { model, list_add, list_update, list_delete, datatable, waiting } = storeToRefs(store_muahang);

const submit = async () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }

  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.mahh) {
        alert("Chưa chọn Mã NVL!");
        return false;
      }
      if (!(product.soluong > 0)) {
        alert("Chưa chọn nhập số lượng");
        return false;
      }
    }
  } else {
    alert("Chưa nhập nguyên liệu!");
    return false;
  }
  model.value.list_add = list_add.value
  model.value.list_update = list_update.value
  model.value.list_delete = list_delete.value
  waiting.value = true;
  var response = await muahangApi.save(model.value);
  waiting.value = false;
  if (response.success) {
    toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
    // location.reload();
    store_muahang.load_data(model.value.id);
  }
  return true;
  // console.log(response)

};

const baogia = () => {
  waiting.value = true;
  muahangApi.baogia(model.value.id).then((response) => {
    waiting.value = false;
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      // location.reload();

      store_muahang.load_data(model.value.id);
    }
  });
}
const readonly = ref(false);
onMounted(() => {
  if ([9, 10, 11].indexOf(model.value.status_id) != -1) {
    readonly.value = true;
  }
})
</script>

<style lang="scss"></style>
