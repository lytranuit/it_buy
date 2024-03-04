<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <AlertError :message="messageError" v-if="messageError" />
        <AlertSuccess :message="messageSuccess" v-if="messageSuccess" />
        <section class="card card-fluid">
          <div class="card-header">
            <div class="d-inline-block w-100">

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
                    <input class="form-control" v-model="model.name" required :readonly="model.status_id != 1">
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Hạn giao hàng:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <Calendar v-model="model.date" dateFormat="yy-mm-dd" class="date-custom" :manualInput="false" showIcon
                      :minDate="minDate" :readonly="model.status_id != 1" />
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Nguyên vật liệu:<i class="text-danger">*</i></b>
                  <div class="col-12 col-lg-12 pt-1">
                    <FormDutruChitiet></FormDutruChitiet>
                  </div>
                </div>
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label">Ghi chú:</b>
                  <div class="col-12 col-lg-12 pt-1">
                    <textarea class="form-control form-control-sm" v-model="model.note"
                      :readonly="model.status_id != 1"></textarea>
                  </div>
                </div>
              </div>
              <div class="col-md-12 text-center" v-if="model.status_id == 1">
                <Button label="Lưu lại" icon="pi pi-save" class="p-button-success p-button-sm mr-2"
                  @click.prevent="submit()"></Button>
                <Button label="Xuất PDF" icon="pi pi-file" class="p-button-sm mr-2" @click.prevent="xuatpdf()"></Button>
              </div>

            </div>
          </div>
        </section>
      </form>
    </div>
  </div>

  <hr class="mb-5">
  <div class="row clearfix" v-if="model.status_id != 1">
    <div class="col-12">
      <section class="card card-fluid">
        <div class="card-header">
          <div class="h4 text-center">Trình ký</div>
        </div>
        <div class="card-body">
          <div class="row">
            <div class="col-md-12 text-center">
              <a :href="model.pdf" :download="model.pdf" class="download-icon-link d-inline-flex align-items-center">
                <i class="far fa-file text-danger" style="font-size: 40px; margin-right: 10px;"></i>
                {{ model.pdf }}
              </a>
              <div class="d-inline-block ml-5" v-if="model.status_id == 2">
                <a :href="path_esign + '/admin/document/create?queue_id=' + model.esign_id">
                  <Button label="Bắt đầu trình ký" class="p-button-primary p-button-sm mr-2"
                    icon="fas fa-long-arrow-alt-right"></Button>
                </a>
              </div>

              <div class="d-inline-block ml-5" v-else-if="model.status_id == 3">
                <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                  <Button label="Đang trình ký" class="p-button-warning p-button-sm mr-2"
                    icon="fas fa-spinner fa-spin"></Button>
                </a>
              </div>

              <div class="d-inline-block ml-5" v-else-if="model.status_id == 4">
                <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                  <Button label="Đã hoàn thành" class="p-button-success p-button-sm mr-2"
                    icon="fas fa-thumbs-up"></Button>
                </a>
              </div>

              <div class="d-inline-block ml-5" v-else-if="model.status_id == 5">
                <a :href="path_esign + '/admin/document/details/' + model.esign_id">
                  <Button label="Không duyệt" class="p-button-danger p-button-sm mr-2" icon="fas fa-times"></Button>
                </a>
              </div>
            </div>
          </div>
        </div>
      </section>

    </div>
  </div>

  <Loading :waiting="waiting"></Loading>
</template>

<script setup>

import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";
import { useToast } from "primevue/usetoast";

import Steps from 'primevue/steps';
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Loading from "../../../components/Loading.vue";
import FormDutruChitiet from '../../../components/Datatable/FormDutruChitiet.vue'
import AlertError from "../../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import dutruApi from "../../../api/dutruApi";
import { useDutru } from '../../../stores/dutru';

import { storeToRefs } from "pinia";
import { useGeneral } from "../../../stores/general";
import moment from "moment";
import AlertSuccess from "../../../components/AlertSuccess.vue";
const path_esign = import.meta.env.VITE_ESIGNURL;
const waiting = ref();
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDutru = useDutru();
const store_general = useGeneral();
const router = useRouter();
const messageError = ref();
const messageSuccess = ref();
const store = useAuth();
const { model, datatable, list_add, list_update, list_delete } = storeToRefs(storeDutru);
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
  dutruApi.get(route.params.id).then((res) => {
    var chitiet = res.chitiet;
    res.date = moment(res.date).format("YYYY-MM-DD");
    delete res.chitiet;
    model.value = res;
    datatable.value = chitiet;
  });
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
  model.value.list_update = list_update.value
  model.value.list_delete = list_delete.value
  waiting.value = true;
  dutruApi.save(model.value).then((response) => {
    waiting.value = false;
    messageError.value = "";
    messageSuccess.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};
const xuatpdf = () => {
  waiting.value = true;
  dutruApi.xuatpdf(model.value.id).then((response) => {
    waiting.value = false;
    messageError.value = "";
    messageSuccess.value = "";
    if (response.success) {
      toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Xuất file thành công', life: 3000 });
      location.reload();
    } else {
      messageError.value = response.message;
    }

    // console.log(response)
  });
}
</script>
