<template>
  <DataTable :value="details" sortField="stt" :sortOrder="1">
    <Column field="stt" header="STT">
      <template #body="slotProps">
        <Button
          :label="slotProps.data.stt"
          size="small"
          :severity="checkStatus(slotProps.data)"
        ></Button>
      </template>
    </Column>
    <Column field="manvl" header="Mã NVL">
      <template #body="slotProps">
        <div>
          {{ slotProps.data.manvl }}
        </div>
        <template v-if="slotProps.data.thaythe.length > 0">
          <div
            v-for="item in slotProps.data.thaythe"
            :key="item"
            class="text-info"
          >
            <i class="fas fa-exchange-alt"></i>
            {{ item.manvl }}
          </div>
        </template>
      </template>
    </Column>
    <Column field="tennvl" header="Tên NVL">
      <template #body="slotProps">
        <div>
          {{ slotProps.data.tennvl }}
        </div>
        <template v-if="slotProps.data.thaythe.length > 0">
          <div
            v-for="item in slotProps.data.thaythe"
            :key="item"
            class="text-info"
          >
            <i class="fas fa-exchange-alt"></i>
            {{ item.tennvl }}
          </div>
        </template>
      </template>
    </Column>
    <Column field="me" header="Mẻ">
      <template #body="slotProps">
        <div>
          {{ slotProps.data.me }}
        </div>
        <template v-if="slotProps.data.thaythe.length > 0">
          <div
            v-for="item in slotProps.data.thaythe"
            :key="item"
            class="text-info"
          >
            <i class="fas fa-exchange-alt"></i>
            {{ item.me }}
          </div>
        </template>
      </template>
    </Column>
    <Column field="soluong" header="Số lượng">
      <template #body="slotProps">
        <div>
          {{ formatPrice(slotProps.data.soluong) }} {{ slotProps.data.dvtnvl }}
        </div>
        <template v-if="slotProps.data.thaythe.length > 0">
          <div
            v-for="item in slotProps.data.thaythe"
            :key="item"
            class="text-info"
          >
            <i class="fas fa-exchange-alt"></i>
            {{ formatPrice(item.soluong) }}
            {{ item.dvtnvl }}
          </div>
        </template>
      </template>
    </Column>
    <Column field="tonkho" header="Tồn kho">
      <template #body="slotProps">
        <div>
          {{ formatPrice(slotProps.data.tonkho) }} {{ slotProps.data.dvtnvl }}
        </div>
        <template v-if="slotProps.data.thaythe.length > 0">
          <div
            v-for="item in slotProps.data.thaythe"
            :key="item"
            class="text-info"
          >
            <i class="fas fa-exchange-alt"></i>
            {{ formatPrice(item.tonkho) }}
            {{ item.dvtnvl }}
          </div>
        </template>
      </template>
    </Column>
    <Column field="dutru" header="Dự trù">
      <template #body="slotProps">
        <div v-for="item in slotProps.data.dutru" :key="item">
          <a
            :href="'/dutru/edit/' + item.id"
            class="text-primary"
            target="_blank"
            >{{ item.code }}</a
          >
          <Tag
            value="Đã duyệt"
            size="small"
            severity="success"
            class="ml-2"
            v-if="item.status_id == 4"
          />
          <Tag
            value="Không duyệt"
            size="small"
            severity="danger"
            class="ml-2"
            v-else-if="item.status_id == 5"
          />
          <a
            :href="'/dutru/edit/' + item.id"
            class="text-primary"
            target="_blank"
            v-else
          >
            <Tag
              value="Đang trình ký"
              size="small"
              severity="warning"
              class="ml-2"
            />
          </a>
        </div>
      </template>
    </Column>
    <Column field="muahang" header="Mua hàng">
      <template #body="slotProps">
        <div v-for="item of slotProps.data.muahang" :key="item.id">
          <a
            :href="'/muahang/edit/' + item.id"
            class="text-primary mr-2"
            target="_blank"
            >{{ item.id }} - {{ item.code }}
          </a>
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
                (item['loaithanhtoan'] == 'tra_truoc' && item['is_thanhtoan']))
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
        </div>
      </template>
    </Column>
  </DataTable>
</template>
  <script setup>
import Column from "primevue/column";
import DataTable from "primevue/datatable";
import { onMounted } from "vue";
import { formatPrice } from "../../utilities/util";
import Button from "primevue/button";
import { RouterLink } from "vue-router";
import Tag from "primevue/tag";
const props = defineProps({
  details: { type: Array, default: [] },
});
const checkStatus = (item) => {
  var re = "danger";
  if (item.status) {
    re = "success";
  }

  return re;
};
onMounted(() => {
  // store_qlsx.fetchNhacc();
  // store_qlsx.fetchNhasx();
  // store_qlsx.fetchNVL();
  // loadLazyData();
});
</script>
  