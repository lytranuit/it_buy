<template>
  <Card>
    <template #title>Đánh giá</template>
    <template #content>
      <Button
        label="Thêm"
        icon="pi pi-plus"
        class="p-button-success p-button-sm mr-2"
        @click="addRow"
        v-if="model.status_id == 1 && is_CungungNVL"
      ></Button>
      <div>
        <div class="row" v-for="item in danhgia" :key="item">
          <Divider align="center" type="dotted">
            <div
              class="d-flex align-items-center"
              v-if="!item.date_accept && !item.date_no_accept && is_CungungNVL"
            >
              <input
                class="form-control form-control-sm text-center"
                v-model="item.bophan"
                placeholder="Bộ phận"
              />
              <i
                class="pi pi-trash ml-2 text-danger"
                style="cursor: pointer"
                @click="confirmDelete(item)"
              ></i>
            </div>
            <span v-else>{{ item.bophan }}</span>
          </Divider>
          <Message
            :closable="false"
            v-if="item.date_accept"
            severity="success"
            class="col-12"
            ><b>{{ item?.user?.FullName }}</b> đã chấp nhận vào lúc
            <b>{{ formatDate(item.date_accept, "YYYY-MM-DD HH:mm:ss") }}</b>
            <p v-if="item.note">
              <span style="font-size: 14px">Ý kiến: </span>
              <span class="text-danger">{{ item.note }}</span>
            </p>
          </Message>
          <Message
            :closable="false"
            v-else-if="item.date_no_accept"
            severity="error"
            class="col-12"
            ><b>{{ item?.user?.FullName }}</b> đã không chấp nhận vào lúc
            <b>{{ formatDate(item.date_no_accept, "YYYY-MM-DD HH:mm:ss") }}</b>
            <p v-if="item.note">
              <span style="font-size: 14px">Ý kiến: </span>
              <span class="text-danger">{{ item.note }}</span>
            </p>
          </Message>
          <div class="col-md-12" v-else>
            <div class="d-flex">
              <div style="width: 400px">
                <user-tree-select
                  v-model="item.user_id"
                  placeholder="Người đánh giá"
                  :disabled="!is_CungungNVL"
                ></user-tree-select>
              </div>
              <div class="ml-auto">
                <Button
                  label="Không chấp nhận"
                  severity="danger"
                  size="small"
                  icon="pi pi-times"
                  class="mr-2"
                  @click="Khongchapnhan(item)"
                  v-if="item.user_id == user.id && item.id > 0"
                ></Button>
                <Button
                  label="Chấp nhận"
                  severity="success"
                  size="small"
                  icon="pi pi-check"
                  @click="Chapnhan(item)"
                  v-if="item.user_id == user.id && item.id > 0"
                ></Button>
              </div>
            </div>
            <div>
              <textarea
                v-model="item.note"
                class="form-control form-control-sm mt-2"
                placeholder="Ý kiến"
                :disabled="item.user_id != user.id || !item.id"
                @blur="saveDanhgia(item)"
              ></textarea>
            </div>
          </div>
        </div>
      </div>
    </template>
  </Card>
  <popup-chapnhan></popup-chapnhan>
</template>
<script setup>
import { computed, onMounted, ref } from "vue";
import { useDanhgianhacungcap } from "../../stores/danhgianhacungcap";
import { storeToRefs } from "pinia";
import danhgianhacungcapApi from "../../api/danhgianhacungcapApi";
import Button from "primevue/button";
import Card from "primevue/card";
import Divider from "primevue/divider";
import UserTreeSelect from "../../components/TreeSelect/UserTreeSelect.vue";
import PopupChapnhan from "./PopupChapnhan.vue";
import { useAuth } from "../../stores/auth";
import { useRoute } from "vue-router";
import { rand } from "../../utilities/rand";
import { useConfirm } from "primevue/useconfirm";
import { formatDate } from "../../utilities/util";
import Message from "primevue/message";
const route = useRoute();
const store = useAuth();
const { user, is_CungungNVL } = storeToRefs(store);
const store_danhgianhacungcap = useDanhgianhacungcap();
const { model, danhgia, list_delete } = storeToRefs(store_danhgianhacungcap);
const confirm = useConfirm();

const addRow = () => {
  danhgia.value.push({ ids: rand() });
};
const confirmDelete = (selected) => {
  confirm.require({
    message: "Bạn có chắc muốn xóa?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      danhgia.value = danhgia.value.filter((item) => {
        return selected != item;
      });

      if (!list_delete.value) {
        list_delete.value = [];
      }
      if (!selected.ids) {
        list_delete.value.push(selected);
      }
    },
  });
};
const openChapnhan = (selected) => {
  store_danhgianhacungcap.openChapnhan(selected);
};
const saveDanhgia = (selected) => {
  danhgianhacungcapApi.saveDanhgia(selected).then((res) => {
    // visibleDanhgia.value = false;
    store_danhgianhacungcap.getDanhgia(route.params.id);
  });
};
const Chapnhan = (selected) => {
  confirm.require({
    message: "Bạn có chắc muốn chấp nhận?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      danhgianhacungcapApi.chapnhanDanhgia(selected).then((res) => {
        // visibleDanhgia.value = false;
        store_danhgianhacungcap.getDanhgia(route.params.id);
      });
    },
  });
};
const Khongchapnhan = (selected) => {
  confirm.require({
    message: "Bạn có chắc muốn không chấp nhận?",
    header: "Xác nhận",
    icon: "pi pi-exclamation-triangle",
    accept: () => {
      danhgianhacungcapApi.KhongchapnhanDanhgia(selected).then((res) => {
        // visibleDanhgia.value = false;
        store_danhgianhacungcap.getDanhgia(route.params.id);
      });
    },
  });
};
onMounted(() => {
  store_danhgianhacungcap.getDanhgia(route.params.id);
});
</script>

<style lang="scss"></style>
