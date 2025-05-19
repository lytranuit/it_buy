<template>
  <TreeSelect :options="list_dangbaoche" :multiple="multiple" :normalizer="normalizer" :modelValue="modelValue"
    :name="name" :required="required" :append-to-body="appendToBody" placeholder="Dạng bào chế"
    @update:modelValue="emit('update:modelValue', $event)" zIndex="3000" :disableFuzzyMatching="true" :limit="0"
    :limitText="(count) => 'Lựa chọn: ' + count + ' dạng'">
  </TreeSelect>
</template>
<script setup>
// import the component
import { storeToRefs } from "pinia";
import { onMounted, computed, ref } from "vue";

const props = defineProps({
  appendToBody: {
    type: Boolean,
    default: true,
  },
  modelValue: {
    type: [String, Array, Number],
  },
  multiple: {
    type: Boolean,
    default: false,
  },
  required: {
    type: Boolean,
    default: false,
  },
  name: {
    type: String,
    default: "khuvuc",
  },
});
const emit = defineEmits(["update:modelValue"]);

const normalizer = (node) => {
  // console.log(id);
  return {
    id: node,
    label: node,
  };
};
const list_dangbaoche = ref([
  "Viên nén",
  "Viên nén bao phim",
  "Viên nén sủi",
  "Viên nang cứng",
  "Viên nang mềm",
  "Bột cốm",
  "Bột sủi",
  "Mỡ",
  "Kem",
  "Gel",
  "Viên ngậm",
  "Kẹo cứng",
  "Gummy",
  "Hỗn dịch lỏng",
  "Serum",
  "Kem",
  "Dung dịch",
  "Cốm sủi bọt",
  "Cốm placebo"
]);
onMounted(() => { });
</script>