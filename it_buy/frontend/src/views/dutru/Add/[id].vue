<template>
  <div class="row clearfix">
    <div class="col-12">
      <form method="POST" id="form">
        <AlertError :message="messageError" v-if="messageError" />
        <section class="card card-fluid">
          <div class="card-body">
            <div class="row">
              <div class="col-md-6">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Tiêu đề:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <input class="form-control" v-model="model.name" required />
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Bộ phận dự trù:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <DepartmentTreeSelect
                      v-model="model.bophan_id"
                      :clearable="false"
                    >
                    </DepartmentTreeSelect>
                  </div>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Hạn giao hàng:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <Calendar
                      v-model="model.date"
                      dateFormat="yy-mm-dd"
                      class="date-custom"
                      :manualInput="false"
                      showIcon
                      :minDate="minDate"
                    />
                  </div>
                </div>
              </div>
              <div class="col-md-12">
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Lý do mua hàng:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <textarea
                      class="form-control form-control-sm"
                      v-model="model.note"
                      required
                    ></textarea>
                  </div>
                </div>
                <div class="form-group row">
                  <b class="col-12 col-lg-12 col-form-label"
                    >Hàng hóa:<i class="text-danger">*</i></b
                  >
                  <div class="col-12 col-lg-12 pt-1">
                    <FormDutruChitiet></FormDutruChitiet>
                  </div>
                </div>
              </div>
              <div class="col-md-12 text-center">
                <Button
                  label="Lưu lại"
                  icon="pi pi-save"
                  class="p-button-success p-button-sm mr-2"
                  @click.once="submit()"
                ></Button>
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

import Steps from "primevue/steps";
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import FormDutruChitiet from "../../../components/Datatable/FormDutruChitiet.vue";
import AlertError from "../../../components/AlertError.vue";
import { useRoute, useRouter } from "vue-router";
import dutruApi from "../../../api/dutruApi";
import { useDutru } from "../../../stores/dutru";
import { storeToRefs } from "pinia";
import { useToast } from "primevue/usetoast";
import { rand } from "../../../utilities/rand";
import DepartmentTreeSelect from "../../../components/TreeSelect/DepartmentTreeSelect.vue";
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDutru = useDutru();
const router = useRouter();
const messageError = ref();
const store = useAuth();
const { model, datatable, list_add } = storeToRefs(storeDutru);
onMounted(() => {
  storeDutru.reset();
  model.value.type_id = route.params.id;
  model.value.status_id = 1;
  if (model.value.type_id == 1) {
    model.value.bophan_id = 19;
  } else if (model.value.type_id == 3) {
    model.value.bophan_id = 4;
  }
  addRow();
});
const addRow = () => {
  let stt = 0;
  if (datatable.value.length) {
    stt = datatable.value[datatable.value.length - 1].stt;
  }
  stt++;
  datatable.value.push({ ids: rand(), stt: stt, soluong: 1 });
};
const submit = () => {
  if (!model.value.name) {
    alert("Chưa nhập tiêu đề!");
    return false;
  }
  if (!model.value.note) {
    alert("Chưa nhập lý do mua hàng!");
    return false;
  }
  if (!model.value.date) {
    alert("Chưa chọn hạn giao hàng!");
    return false;
  }
  if (datatable.value.length) {
    for (let product of datatable.value) {
      if (!product.tenhh) {
        alert("Chưa nhập hàng hóa!");
        return false;
      }
      if (model.value.type_id == 1) {
        if (!product.grade) {
          alert("Chưa nhập Grade!");
          return false;
        }
        if (!product.tensp) {
          alert("Chưa nhập Tên SP!");
          return false;
        }
        if (!product.dangbaoche) {
          alert("Chưa nhập Dạng bào chế!");
          return false;
        }
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
  model.value.list_add = list_add.value;

  var params = model.value;
  $(".hinhanh-file-input").each(function (index) {
    // console.log(this)
    var files = $(this)[0].files;
    var key = $(this).data("key");
    for (var stt = 0; stt < files.length; stt++) {
      var file = files[stt];
      params["file_" + key + "_" + stt] = file;
    }
  });

  dutruApi.save(params).then((response) => {
    messageError.value = "";
    if (response.success) {
      toast.add({
        severity: "success",
        summary: "Thành công!",
        detail: "Tạo dự trù thành công",
        life: 3000,
      });
      router.push("/dutru/edit/" + response.id);
    } else {
      messageError.value = response.message;
    }
    // console.log(response)
  });
};
</script>
