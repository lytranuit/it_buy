<template>
  <div>
    <div class="row clearfix">
      <div class="col-12">
        <div class="card mb-2">
          <div class="card-body">
            <div class="flex-m">
              <h5 class="title">
                <span>
                  {{ model.name }}</span>
              </h5>

            </div>
            <div class="flex-m"><span class=""><span class="">ID</span>: <span class="font-weight-bold">{{ model.code
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
        <section class="card card-fluid">
          <TabView v-model:activeIndex="tabviewActive">
            <TabPanel>
              <template #header>
                Quy trình
              </template>
              <Stepper orientation="vertical" :activeStep="activeStep">
                <StepperPanel header="Đề nghị mua hàng">
                  <template #content="{ nextCallback }">
                    <Form></Form>
                  </template>
                </StepperPanel>
                <StepperPanel header="Báo giá">

                  <template #content="{ prevCallback, nextCallback }">
                    <Baogia></Baogia>
                  </template>
                </StepperPanel>
                <StepperPanel header="So sánh báo giá">

                  <template #content="{ prevCallback }">
                    <Sosanhbaogia></Sosanhbaogia>
                  </template>
                </StepperPanel>
                <StepperPanel header="Trình ký">

                  <template #content="{ prevCallback }">
                    <Trinhky></Trinhky>
                  </template>
                </StepperPanel>

                <StepperPanel header="Đơn đặt hàng" v-if="model.status_id == 10">

                  <template #content="{ prevCallback, nextCallback }">
                    <Dondathang @next="nextCallback"></Dondathang>
                  </template>
                </StepperPanel>

                <StepperPanel header="Đề nghị thanh toán"
                  v-if="model.status_id == 10 && model.loaithanhtoan == 'tra_truoc'">

                  <template #content="{ prevCallback, nextCallback }">
                    <Thanhtoan @next="nextCallback"></Thanhtoan>
                  </template>
                </StepperPanel>

                <StepperPanel header="Nhận hàng" v-if="model.status_id == 10">

                  <template #content="{ prevCallback }">
                    <Nhanhang></Nhanhang>
                  </template>
                </StepperPanel>

                <StepperPanel header="Đề nghị thanh toán "
                  v-if="model.status_id == 10 && model.loaithanhtoan == 'tra_sau'">

                  <template #content="{ prevCallback, nextCallback }">
                    <Thanhtoan @next="nextCallback"></Thanhtoan>
                  </template>
                </StepperPanel>
                <StepperPanel header="Hoàn thành" v-if="model.date_finish">

                  <template #content="{ prevCallback }">
                    <div class="row text-center">

                      <div class="col-md-12">
                        <img src="/src/assets/images/Purchase_Success.png" />
                      </div>
                      <div class="col-md-12 ">
                        <b class="text-success">Hoàn thành</b>
                      </div>
                    </div>
                  </template>

                </StepperPanel>
              </Stepper>
            </TabPanel>
            <TabPanel>
              <template #header>
                Files
              </template>
              <MuahangFiles></MuahangFiles>
            </TabPanel>
          </TabView>

        </section>
      </div>

    </div>
    <div class="row mt-2" v-if="model.id > 0">
      <div class="col-md-12">
        <Comment></Comment>
      </div>
    </div>

    <Loading :waiting="waiting"></Loading>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { onMounted, computed } from "vue";

import Stepper from 'primevue/stepper';
import StepperPanel from 'primevue/stepperpanel';
import TabView from 'primevue/tabview';
import TabPanel from 'primevue/tabpanel';
import Loading from "../../../components/Loading.vue";
import { useRoute, useRouter } from "vue-router";
import muahangApi from "../../../api/muahangApi";
import { useMuahang } from '../../../stores/muahang';

import { storeToRefs } from "pinia";
import moment from "moment";
import Comment from "../../../components/muahang/Comment.vue";
import MuahangFiles from "../../../components/Datatable/MuahangFiles.vue";
import Form from "../../../components/muahang/Form.vue";
import Baogia from "../../../components/muahang/Baogia.vue";
import Sosanhbaogia from "../../../components/muahang/Sosanhbaogia.vue";
import Trinhky from "../../../components/muahang/Trinhky.vue";
import Dondathang from "../../../components/muahang/Dondathang.vue";
import Nhanhang from "../../../components/muahang/Nhanhang.vue";
import Thanhtoan from "../../../components/muahang/Thanhtoan.vue";
import { formatDate } from "../../../utilities/util";
const activeStep = computed(() => {
  var value = 0;
  if (model.value.status_id == 6) {
    value = 1;
  } else if (model.value.status_id == 7) {
    value = 2;
  } else if ([8, 9, 11].indexOf(model.value.status_id) != -1) {
    value = 3;
  } else if ([10].indexOf(model.value.status_id) != -1) {
    value = 4;
  }
  if (model.value.is_dathang) {
    value++;
  }
  if (model.value.is_thanhtoan) {
    value++;
  }
  if (model.value.date_finish) {
    value = 7;
  }
  return value;
});
const route = useRoute();
const storeMuahang = useMuahang();
const { model, waiting, tabviewActive, user_created_by } = storeToRefs(storeMuahang);





onMounted(() => {
  storeMuahang.load_data(route.params.id);
});
</script>
