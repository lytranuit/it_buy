<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <AlertError :message="messageError" v-if="messageError" />
        <section class="card card-fluid">
          <div class="card-header">
            <div class="d-inline-block w-100">
              <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2"
                @click.prevent="submit()"></Button>
            </div>
          </div>
          <div class="card-body">
            <div class="row">
              <div class="col-12">
                <Steps :model="items" />
              </div>
              <div class="col-md-9">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <input class="form-control" v-model="model.name" required>
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Hạn giao hàng:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false" showIcon
                      :minDate="minDate" />
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Hàng hóa:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <FormDutruChitiet></FormDutruChitiet>
                  </div>
                </div>
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                  <div class="col-12 col-lg-12 pt-1">
                    <textarea class="form-control form-control-sm" v-model="model.note"></textarea>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";

import Steps from 'primevue/steps';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import FormDutruChitiet from '../../../components/Datatable/FormDutruChitiet.vue'
import AlertError from "../../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import dutruApi from "../../../api/dutruApi";
import { useDutru } from '../../../stores/dutru';
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDutru = useDutru();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const { model, datatable, list_add } = storeToRefs(storeDutru);
const items = ref([
  {
    label: 'Dự trù'
  },
  {
    label: 'Trình ký'
  },
  {
    label: 'Đề nghị mua hàng'
  },
  {
    label: 'Trình ký'
  },
  {
    label: 'Đơn mua hàng'
  },
  {
    label: 'Nhận hàng'
  },
  {
    label: 'Hoàn thành'
  }
]);
onMounted(() => {
  storeDutru.reset();
  model.value.type_id = route.params.id;
  model.value.status_id = 1;
});
const submit = () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn hạn giao hàng!");
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.mahh) {
        alert("Chưa chọn Mã NVL!");
        return false;
      }
      if (!product.dvt) {
        alert("Chưa nhập đơn vị tính!");
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
  dutruApi.save(model.value).then((response) => {
    messageError.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Tạo dự trù thành công', life: 3000 });
      router.push("/dutru/edit/" + response.id);
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};
</script>
