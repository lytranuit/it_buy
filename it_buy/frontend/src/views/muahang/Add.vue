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
                <Steps :model="items" v-model:activeStep="model.activeStep" />
              </div>
              <div class="col-md-9">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Tiêu đề:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <input class="form-control" v-model="model.name" required>
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Lý do mua hàng:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <textarea class="form-control" v-model="model.note" required></textarea>
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Nguyên vật liệu:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <FormMuahangChitiet></FormMuahangChitiet>
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
import { useAuth } from "../../stores/auth";

import Steps from 'primevue/steps';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import FormMuahangChitiet from '../../components/Datatable/FormMuahangChitiet.vue'
import AlertError from "../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import muahangApi from "../../api/muahangApi";
import { useMuahang } from '../../stores/muahang';
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
const toast = useToast();
const route = useRoute();
const storeMuahang = useMuahang();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const { model, datatable, list_add } = storeToRefs(storeMuahang);
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
  // console.log(route.query)
  model.value = {};
  if (!datatable.value.length) {
    router.push("/muahang");
  }
  model.value.status_id = 1;
  model.value.activeStep = 2;
});
const submit = () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }
  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  // console.log(datatable.value);
  // return false;
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
  model.value.list_add = list_add.value;
  // console.log(model.value);

  muahangApi.save(model.value).then(async (response) => {
    messageError.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Tạo thành công', life: 3000 });
      router.push("/muahang/edit/" + response.id);
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};
</script>
