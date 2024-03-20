<template>
  <div class="row clearfix">
    <div class="col-12">
      <div class="card mb-2">
        <div class="card-body">
          <div class="flex-m">
            <h5 class="title">
              <span>
                Đánh giá nguyên vật liệu {{ model?.material?.tenhh }} của {{ model?.ncc?.tenncc }}</span>
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
    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>

import { ref } from "vue";
import { onMounted, computed } from "vue";
import { useAuth } from "../../../stores/auth";
import { useToast } from "primevue/usetoast";
import TabView from 'primevue/tabview';
import TabPanel from 'primevue/tabpanel';
import Button from 'primevue/button';
import Message from 'primevue/message';
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

const confirm = useConfirm();
const store_auth = useAuth();
const { is_LeadQa, user } = storeToRefs(store_auth);
const toast = useToast();
const minDate = new Date();
const route = useRoute();
const storeDanhgianhacungcap = useDanhgianhacungcap();
const { model, waiting, user_created_by } = storeToRefs(storeDanhgianhacungcap);

onMounted(() => {
  danhgianhacungcapApi.get(route.params.id).then((res) => {
    var user = res.user_created_by;
    // res.date = moment(res.date).format("YYYY-MM-DD");
    delete res.user_created_by;
    user_created_by.value = user;
    model.value = res;
  });
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
        } else {
          toast.add({ severity: 'danger', summary: 'Lỗi!', detail: '', life: 3000 });
        }
      });
    }
  });

};
</script>
<style>
.p-Panel .p-Panel -toggleable .p-Panel -header {
  background-color: transparent;
  border: 0;
}
</style>
