<template>
  <Button label="Tạo mới" icon="pi pi-plus" class="p-button-success p-button-sm mr-2" @click="visible = true"></Button>

  <Dialog v-model:visible="visible" modal header="Chọn loại hàng hóa" style="width: 500px;"
    :breakpoints="{ '1199px': '75vw', '575px': '95vw' }">
    <div class="row">
      <div class="col-12">
        <div class="process-group" id="accordionselect">
          <template v-for="group in groups" :key="group.id">
            <a class="process d-block" @click="execute(group.id)">
              {{ group.name }}
            </a>
          </template>
        </div>
      </div>
    </div>
  </Dialog>
</template>

<script setup>
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
const router = useRouter();
const visible = ref();
const groups = ref([
  {
    id: 2,
    name: "Mua hàng gián tiếp"
  },
  {
    id: 3,
    name: "Hóa chất, thuốc thử QC"
  },
  {
    id: 1,
    name: "Nguyên vật liệu"
  },
]);
const execute = (id) => {
  visible.value = false;
  router.push(
    "/dutru/add/" + id
  );
};
onMounted(() => {

});
</script>
<style scoped>
.process-group .process {
  margin-left: 20px;
  padding: 10px;
  margin-bottom: 5px;
  cursor: pointer;
}

.process-group .process:hover {
  background: aliceblue;
}
</style>