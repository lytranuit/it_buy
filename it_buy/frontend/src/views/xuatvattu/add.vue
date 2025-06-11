<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <AlertError :message="messageError" v-if="messageError" />
        <section class="card card-fluid">
          <div class="card-body">
            <div class="row">
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Bộ phận đề nghị:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <KhoTreeSelect v-model="model.bophan_id" :multiple="false">
                    </KhoTreeSelect>
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Ngày mong muốn xuất:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false"
                      showIcon :minDate="minDate" />
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Lý do:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <textarea class="form-control form-control-sm" v-model="model.note" required></textarea>
                  </div>
                </div>
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Vật tư:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <FormxuatvattuChitiet></FormxuatvattuChitiet>
                  </div>
                </div>
              </div>

            </div>
          </div>
          <div class="card-footer text-center">
            <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2" @click="submit()"
              :disabled="buttonDisabled"></Button>
          </div>
        </section>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../stores/auth";

import Button from "primevue/button";
import Calendar from "primevue/calendar";
import FormxuatvattuChitiet from "../../components/xuatvattu/FormxuatvattuChitiet.vue";
import AlertError from "../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import xuatvattuApi from "../../api/xuatvattuApi";
import { usexuatvattu } from "../../stores/xuatvattu";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { rand } from "../../utilities/rand";
import { useGeneral } from "../../stores/general";
import KhoTreeSelect from "../../components/TreeSelect/KhoTreeSelect.vue";
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storexuatvattu = usexuatvattu();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const store_general = useGeneral();
const buttonDisabled = ref();
const { model, datatable, list_add } = storeToRefs(storexuatvattu);
onMounted(() => {
  storexuatvattu.reset();
  store_general.fetchTonkhoVattu();

  model.value.type_id = route.params.id;
  model.value.status_id = 1;
  addRow();
});

const addRow = () => {
  let stt = 0;
  if (datatable.value.length) {
    stt = datatable.value[datatable.value.length - 1].stt;
  }
  stt++;
  datatable.value.push({ ids: rand(), stt: stt, is_new: true });
};


const submit = () => {
  buttonDisabled.value = true;

  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    buttonDisabled.value = false;
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn ngày mong muốn xuất!");
    buttonDisabled.value = false;
    return false;
  }
  if (!model.value.bophan_id) {
    alert("Chưa chọn bộ phận đề nghị!");
    buttonDisabled.value = false;
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {

      if (!product.mahh) {
        alert("Chưa có mã hàng hóa!");
        buttonDisabled.value = false;
        return false;
      }

      if (!product.tenhh) {
        alert("Chưa nhập hàng hóa!");
        buttonDisabled.value = false;
        return false;
      }
      if (!(product.soluong > 0)) {
        alert("Chưa chọn nhập số lượng");
        buttonDisabled.value = false;
        return false;
      }

      if (product.soluong > product.tonkho) {
        alert(product.mahh + " không đủ số lượng xuất!");
        buttonDisabled.value = false;
        return false;
      }
    }
  } else {
    alert("Chưa nhập nguyên liệu!");
    buttonDisabled.value = false;
    return false;
  }
  model.value.list_add = list_add.value;

  var params = model.value;

  xuatvattuApi.save(params).then((response) => {
    messageError.value = "";
    if (response.success) {
      toast.add({
        severity: "success",
        summary: "Thành công!",
        detail: "Tạo đề nghị thành công",
        life: 3000,
      });
      router.push("/xuatvattu/edit/" + response.id);
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};


</script>
