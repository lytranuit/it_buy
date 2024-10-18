<template>
  <div>
    <table class="table table-bordered">
      <thead>
        <tr>
          <th>Đơn hàng của NCC</th>
          <th>Tình trạng</th>
          <th>Chi tiết</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(item, key) in donhang" :key="key">
          <td>
            {{ item.muahang_chonmua.ncc.mancc }} -
            {{ item.muahang_chonmua.ncc.tenncc }}
          </td>
          <td>
            <Tag
              value="Hoàn thành"
              severity="success"
              v-if="item['date_finish']"
            />
            <Tag
              value="Chờ nhận hàng"
              severity="info"
              v-else-if="
                item['is_dathang'] &&
                ((item['loaithanhtoan'] == 'tra_sau' && !item['is_nhanhang']) ||
                  (item['loaithanhtoan'] == 'tra_truoc' &&
                    item['is_thanhtoan']))
              "
            />
            <Tag
              value="Chờ thanh toán"
              severity="info"
              v-else-if="
                item['is_dathang'] &&
                ((item['loaithanhtoan'] == 'tra_truoc' &&
                  !item['is_thanhtoan']) ||
                  (item['loaithanhtoan'] == 'tra_sau' && item['is_nhanhang']))
              "
            />
            <Tag
              value="Đang thực hiện"
              severity="secondary"
              v-else-if="
                item['status_id'] == 1 ||
                item['status_id'] == 6 ||
                item['status_id'] == 7
              "
            />
            <Tag
              value="Đang trình ký"
              severity="warning"
              v-else-if="item['status_id'] == 9"
            />

            <Tag value="Đang đặt hàng" v-else-if="item['status_id'] == 10" />
            <Tag
              value="Không duyệt"
              severity="danger"
              v-else-if="item['status_id'] == 11"
            />
          </td>
          <td>
            <a :href="'/muahang/edit/' + item.id" target="_blank">Link</a>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<script setup>
import { onMounted, ref } from "vue";
import muahangApi from "../../api/muahangApi";
import { useToast } from "primevue/usetoast";
import { useRoute } from "vue-router";
import Tag from "primevue/tag";
const toast = useToast();

const readonly = ref(false);
const route = useRoute();
const donhang = ref([]);
const load = async (id) => {
  var res = await muahangApi.getDonhang(id);

  donhang.value = res;
};
onMounted(() => {
  load(route.params.id);
});
</script>

