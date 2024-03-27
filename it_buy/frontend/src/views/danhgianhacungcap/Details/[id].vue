<template>
  <div class="row clearfix">
    <div class="col-12">
      <div class="card mb-2">
        <div class="card-body">
          <div class="flex-m">
            <h5 class="title">
              <span>
                Đánh giá nguyên vật liệu {{ model?.tenhh }}</span>
            </h5>

          </div>
          <div class="flex-m"><span class=""><span class="">ID</span>: <span class="font-weight-bold">{{ model.id
                }}</span></span><span class="mx-2">|</span>
            <div class=""><span class="">Người tạo</span>: <span class="font-weight-bold">{{ user_created_by.fullName
                }}</span></div><span class="mx-2">|</span>
            <div class=""><span class=""> Ngày tạo: </span><span class="font-weight-bold">{{
                  formatDate(model.created_at) }}</span></div>
          </div>
        </div>
      </div>
    </div>
    <div class="col-12">
      <section class="card">
        <div class="card-body">
          <div class="row mb-2">
            <div class="field col">
              <label for="name">Tên nguyên liệu <span class="text-danger">*</span></label>
              <div>
                <InputText v-model="model.tenhh" class="form-control form-control-sm" :readonly="model.is_chapnhan" />
              </div>
            </div>
            <div class="field col">
              <label for="name">ĐVT <span class="text-danger">*</span></label>
              <div>
                <InputText v-model="model.dvt" class="form-control form-control-sm" :readonly="model.is_chapnhan" />
              </div>
            </div>
            <div class="field col">
              <label for="name">Grade <span class="text-danger">*</span></label>
              <div>
                <InputText v-model="model.grade" class="form-control form-control-sm" :readonly="model.is_chapnhan" />
              </div>
            </div>
          </div>
          <div class="row mb-2">
            <div class="field col">
              <label for="name">Nhà sản xuất <span class="text-danger">*</span></label>
              <div>
                <NsxTreeSelect v-model="model.mansx" :readonly="model.is_chapnhan"></NsxTreeSelect>
              </div>
            </div>
            <div class="field col">
              <label for="name">Nhà phân phối <span class="text-danger">*</span></label>
              <div>
                <NccTreeSelect v-model="model.mancc" :readonly="model.is_chapnhan"></NccTreeSelect>
              </div>
            </div>
          </div>
          <div class="row mb-2">
            <div class="field col">
              <label for="name">Mã số thiết kế</label>
              <div>
                <InputText v-model="model.masothietke" class="form-control form-control-sm"
                  :readonly="model.is_chapnhan" />
              </div>
            </div>
            <div class="field col">
              <label for="name">Qui cách</label>
              <div>
                <InputText v-model="model.quicach" class="form-control form-control-sm" :readonly="model.is_chapnhan" />
              </div>
            </div>
          </div>
          <div class="row mt-5" v-if="!model.is_chapnhan">
            <div class="col-12 text-center">
              <Button label="Thông báo" icon="far fa-paper-plane" size="small" class="mr-2"
                @click="openThongbao"></Button>
              <Button label="Lưu lại" icon="pi pi-check" size="small" severity="success" @click="save"></Button>
            </div>
          </div>
        </div>
      </section>
    </div>
    <div class="col-12">
      <section class="card">
        <div class="card-body">
          <DanhgianhacungcapFiles></DanhgianhacungcapFiles>
          <div class="text-center mt-3">
            <Message :closable="false" v-if="model.is_chapnhan" severity="success">{{ model?.user_chapnhan?.fullName }}
              đã chấp nhận vào lúc {{ formatDate(model.date_chapnhan, "YYYY-MM-DD HH:mm:ss") }}</Message>
            <Button label="Chấp nhân" severity="success" size="small" icon="pi pi-check" v-else-if="is_LeadQa"
              @click="chapnhan"></Button>
          </div>
        </div>
      </section>
    </div>
    <div class="col-md-12 mt-2" v-if="model.id > 0">
      <Comment></Comment>
    </div>
    <Dialog v-model:visible="visibleDialog" header="Thông báo" :modal="true" style="width: 75vw;"
      :breakpoints="{ '1199px': '75vw', '575px': '95vw' }">
      <div class="row">
        <b class="col-12">Người nhận:</b>
        <div class="col-12 mt-2">
          <UserDepartmentTreeSelect multiple required v-model="listuser" :name="'user_thongbao'">
          </UserDepartmentTreeSelect>
        </div>
      </div>
      <template #footer>
        <Button label="Thông báo" icon="pi pi-check" class="p-button-text" @click="thongbao" size="small"></Button>
      </template>
    </Dialog>
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>

import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";
import { useToast } from "primevue/usetoast";
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import Message from 'primevue/message';
import Dialog from 'primevue/dialog';
import Loading from "../../../components/Loading.vue";
import { useRoute, useRouter } from "vue-router";
import danhgianhacungcapApi from "../../../api/danhgianhacungcapApi";
import { useDanhgianhacungcap } from '../../../stores/danhgianhacungcap';

import { storeToRefs } from "pinia";
import moment from "moment";
import { formatDate } from "../../../utilities/util";
import Comment from "../../../components/danhgianhacungcap/Comment.vue";
import DanhgianhacungcapFiles from "../../../components/Datatable/DanhgianhacungcapFiles.vue";
import { useConfirm } from "primevue/useconfirm";
import NccTreeSelect from "../../../components/TreeSelect/NccTreeSelect.vue";
import NsxTreeSelect from "../../../components/TreeSelect/NsxTreeSelect.vue";
import UserDepartmentTreeSelect from "../../../components/TreeSelect/UserDepartmentTreeSelect.vue";


const confirm = useConfirm();
const store_auth = useAuth();
const { is_LeadQa, user } = storeToRefs(store_auth);
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDanhgianhacungcap = useDanhgianhacungcap();
const { model, waiting, user_created_by } = storeToRefs(storeDanhgianhacungcap);

const loag_data = (id) => {
  danhgianhacungcapApi.get(id).then((res) => {
    var user = res.user_created_by;
    // res.date = moment(res.date).format("YYYY-MM-DD");
    delete res.user_created_by;
    user_created_by.value = user;
    model.value = res;
  });
}
onMounted(() => {
  loag_data(route.params.id);
});
const chapnhan = () => {
  confirm.require({
    message: 'Chấp nhận nhà cung cấp này!',
    header: 'Xác nhận',
    icon: 'pi pi-exclamation-triangle',
    accept: () => {

      waiting.value = true;
      danhgianhacungcapApi.chapnhan(model.value.id).then((response) => {
        waiting.value = false;
        if (response.success) {
          toast.add({ severity: 'success', summary: 'Thành công!', detail: 'Thay đổi thành công', life: 3000 });
          loag_data();
        } else {
          toast.add({ severity: 'danger', summary: 'Lỗi!', detail: '', life: 3000 });
        }
      });
    }
  });

};

const save = () => {
  if (!valid()) {
    alert("Nhập đầy dủ thông tin!")
    return false;
  }
  // waiting.value = true;
  danhgianhacungcapApi.save(model.value).then((res) => {
    // waiting.value = false;
    if (res.success) {
      toast.add({
        severity: "success",
        summary: "Thành công",
        detail: "Thành công",
        life: 3000,
      });
      emit("save", res.data);
    } else {
      toast.add({
        severity: "error",
        summary: "Lỗi",
        detail: res.message,
        life: 3000,
      });
    }
    // loadLazyData();
  });
};

///Form
const valid = () => {
  if (!model.value.tenhh) return false;
  if (!model.value.dvt) return false;
  if (!model.value.mancc) return false;
  if (!model.value.mansx) return false;
  return true;
};
const listuser = ref([]);
const visibleDialog = ref();
const openThongbao = async () => {

  // listuser.value = await muahangApi.getUserNhanhang(model.value.id);
  visibleDialog.value = true;
}
const thongbao = async () => {
  visibleDialog.value = false;
  var res = await danhgianhacungcapApi.thongbao({ danhgianhacungcap_id: model.value.id, list_user: listuser.value });
  if (res.success) {
    toast.add({
      severity: "success",
      summary: "Thành công",
      detail: "Thành công",
      life: 3000,
    });
  }
}
</script>
<style>
.p-Panel .p-Panel -toggleable .p-Panel -header {
  background-color: transparent;
  border: 0;
}
</style>
