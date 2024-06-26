<template>
  <TreeSelect
    :options="DutruChitiet"
    :multiple="multiple"
    :normalizer="normalizer"
    :modelValue="modelValue"
    :name="name"
    :required="required"
    :append-to-body="appendToBody"
    @update:modelValue="emit('update:modelValue', $event)"
    zIndex="3000"
    :disableFuzzyMatching="true"
  >
    <template
      #option-label="{
        node,
        // shouldShowCount,
        // count,
        labelClassName,
        // countClassName,
      }"
    >
      <label :class="labelClassName">
        <div class="d-flex py-2">
          {{ node.label
          }}<Tag
            v-if="node.raw.is_danhgia"
            severity="success"
            value="Đã đánh giá"
            class="ml-auto"
          />
          <Tag
            v-else
            severity="danger"
            value="Chưa đánh giá"
            class="ml-auto"
            @click="alert1(node)"
          />
        </div>
      </label>
    </template>
  </TreeSelect>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import Api from "../../api/Api";
import Tag from "primevue/tag";
const props = defineProps({
  appendToBody: {
    type: Boolean,
    default: true,
  },
  modelValue: {
    type: [String, Array],
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
    default: "dutru_chitiet",
  },
  type_id: Number,
});
const emit = defineEmits(["update:modelValue"]);
const DutruChitiet = ref([]);
const alert1 = (node) => {
  console.log(node);
};
const normalizer = (node) => {
  return {
    id: node.id,
    label: node.mahh ? node.mahh + " - " + node.tenhh : node.tenhh,
  };
};
onMounted(() => {
  Api.dutruchitiet({ type_id: props.type_id }).then((response) => {
    DutruChitiet.value = response;
    return response;
  });
});
</script>
